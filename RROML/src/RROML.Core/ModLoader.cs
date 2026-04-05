using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RROML.Abstractions;

namespace RROML.Core
{
    internal sealed class ModLoader
    {
        private readonly LoaderConfig _config;
        private readonly FileLogger _logger;
        private readonly string _gameRootPath;
        private readonly string _loaderPath;
        private readonly string _modsPath;

        public ModLoader(LoaderConfig config, FileLogger logger, string gameRootPath, string loaderPath, string modsPath)
        {
            _config = config;
            _logger = logger;
            _gameRootPath = gameRootPath;
            _loaderPath = loaderPath;
            _modsPath = modsPath;
        }

        public ModLoadSummary LoadAll()
        {
            var summary = new ModLoadSummary();
            var candidates = FindCandidates();
            summary.CandidateCount = candidates.Count;
            _logger.Info("Found " + candidates.Count + " mod candidate(s).");

            foreach (var candidate in OrderCandidates(candidates))
            {
                try
                {
                    summary.LoadedModCount += LoadOne(candidate);
                }
                catch (Exception exception)
                {
                    summary.FailedModCount++;
                    _logger.Error("Failed to load mod assembly: " + candidate.AssemblyPath, exception);
                }
            }

            return summary;
        }

        private List<ModCandidate> FindCandidates()
        {
            var result = new List<ModCandidate>();
            if (!Directory.Exists(_modsPath))
            {
                Directory.CreateDirectory(_modsPath);
                return result;
            }

            foreach (var dllPath in Directory.GetFiles(_modsPath, "*.dll", SearchOption.TopDirectoryOnly))
            {
                result.Add(new ModCandidate
                {
                    ModRoot = _modsPath,
                    AssemblyPath = dllPath,
                    Manifest = null
                });
            }

            foreach (var directory in Directory.GetDirectories(_modsPath))
            {
                var manifestPath = Path.Combine(directory, "mod.json");
                var manifest = SimpleJson.ReadFile<ModManifest>(manifestPath);
                if (manifest == null || string.IsNullOrWhiteSpace(manifest.EntryDll))
                {
                    var guessedDll = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    if (guessedDll == null)
                    {
                        _logger.Warn("Skipping folder with no mod.json entry DLL and no top-level DLL: " + directory);
                        continue;
                    }

                    result.Add(new ModCandidate
                    {
                        ModRoot = directory,
                        AssemblyPath = guessedDll,
                        Manifest = manifest
                    });
                    continue;
                }

                var assemblyPath = Path.Combine(directory, manifest.EntryDll);
                if (!File.Exists(assemblyPath))
                {
                    _logger.Warn("Skipping folder mod because entry DLL is missing: " + assemblyPath);
                    continue;
                }

                result.Add(new ModCandidate
                {
                    ModRoot = directory,
                    AssemblyPath = assemblyPath,
                    Manifest = manifest
                });
            }

            return result;
        }

        private List<ModCandidate> OrderCandidates(List<ModCandidate> candidates)
        {
            var ordered = new List<ModCandidate>();
            var byId = new Dictionary<string, ModCandidate>(StringComparer.OrdinalIgnoreCase);
            var visited = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var candidate in candidates)
            {
                var candidateId = GetCandidateId(candidate);
                if (byId.ContainsKey(candidateId))
                {
                    _logger.Warn("Duplicate mod id detected. Keeping first and skipping later candidate: " + candidateId);
                    continue;
                }

                byId[candidateId] = candidate;
            }

            foreach (var pair in byId)
            {
                Visit(pair.Key, byId, visited, ordered);
            }

            return ordered;
        }

        private void Visit(string candidateId, Dictionary<string, ModCandidate> byId, Dictionary<string, int> visited, List<ModCandidate> ordered)
        {
            int state;
            if (visited.TryGetValue(candidateId, out state))
            {
                if (state == 1)
                {
                    _logger.Warn("Dependency cycle detected while ordering mods around " + candidateId + ". Continuing with best-effort order.");
                }
                return;
            }

            ModCandidate candidate;
            if (!byId.TryGetValue(candidateId, out candidate))
            {
                return;
            }

            visited[candidateId] = 1;
            foreach (var dependencyId in GetDependencies(candidate))
            {
                ModCandidate dependency;
                if (!byId.TryGetValue(dependencyId, out dependency))
                {
                    _logger.Warn("Mod " + candidateId + " declares missing dependency " + dependencyId + ". It will still be attempted after available dependencies.");
                    continue;
                }

                Visit(dependencyId, byId, visited, ordered);
            }

            visited[candidateId] = 2;
            if (!ordered.Contains(candidate))
            {
                ordered.Add(candidate);
            }
        }

        private int LoadOne(ModCandidate candidate)
        {
            var loadedCount = 0;
            var candidateId = GetCandidateId(candidate);
            var assemblyFileName = Path.GetFileNameWithoutExtension(candidate.AssemblyPath);
            if (IsDisabled(candidate, assemblyFileName) || IsDisabled(candidate, candidateId))
            {
                _logger.Info("Skipping disabled mod: " + assemblyFileName);
                return 0;
            }

            var missingDependencies = GetDependencies(candidate)
                .Where(IsDependencyDisabled)
                .ToArray();
            if (missingDependencies.Length > 0)
            {
                _logger.Warn("Skipping mod " + candidateId + " because dependencies are disabled: " + string.Join(", ", missingDependencies));
                return 0;
            }

            var assembly = Assembly.LoadFrom(candidate.AssemblyPath);
            var modTypes = assembly.GetTypes()
                .Where(type => typeof(IRromlMod).IsAssignableFrom(type) && !type.IsAbstract && type.IsClass)
                .ToArray();

            if (modTypes.Length == 0)
            {
                _logger.Warn("No IRromlMod implementations found in " + candidate.AssemblyPath);
                return 0;
            }

            foreach (var modType in modTypes)
            {
                IRromlMod mod = null;

                try
                {
                    mod = (IRromlMod)Activator.CreateInstance(modType);
                }
                catch (Exception exception)
                {
                    _logger.Error("Could not create mod type " + modType.FullName, exception);
                    continue;
                }

                if (IsDisabled(candidate, mod.Id) || IsDisabled(candidate, mod.Name))
                {
                    _logger.Info("Skipping disabled mod instance: " + mod.Name);
                    continue;
                }

                var configRoot = Path.Combine(_loaderPath, "Configs", SanitizeName(mod.Id ?? mod.Name ?? assemblyFileName));
                var context = new ModContext(_gameRootPath, _loaderPath, _modsPath, configRoot, _logger);

                _logger.Info("Loading mod " + mod.Name + " (" + mod.Version + ")");
                mod.OnLoad(context);
                _logger.Info("Loaded mod " + mod.Name);
                loadedCount++;
            }

            return loadedCount;
        }

        private IEnumerable<string> GetDependencies(ModCandidate candidate)
        {
            if (candidate.Manifest == null || candidate.Manifest.Dependencies == null)
            {
                return new string[0];
            }

            return candidate.Manifest.Dependencies.Where(item => !string.IsNullOrWhiteSpace(item));
        }

        private bool IsDependencyDisabled(string dependencyId)
        {
            return _config.DisabledMods != null && _config.DisabledMods.Any(item => string.Equals(item, dependencyId, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsDisabled(ModCandidate candidate, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || _config.DisabledMods == null)
            {
                return false;
            }

            return _config.DisabledMods.Any(item => string.Equals(item, name, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetCandidateId(ModCandidate candidate)
        {
            if (candidate.Manifest != null && !string.IsNullOrWhiteSpace(candidate.Manifest.Id))
            {
                return candidate.Manifest.Id;
            }

            return Path.GetFileNameWithoutExtension(candidate.AssemblyPath);
        }

        private static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "UnknownMod";
            }

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value;
        }
    }
}
