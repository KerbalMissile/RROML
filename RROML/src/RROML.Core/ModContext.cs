using System.IO;
using RROML.Abstractions;

namespace RROML.Core
{
    internal sealed class ModContext : IModContext
    {
        private readonly string _configRoot;

        public ModContext(string gameRootPath, string loaderPath, string modsPath, string configRoot, IModLogger logger)
        {
            GameRootPath = gameRootPath;
            LoaderPath = loaderPath;
            ModsPath = modsPath;
            _configRoot = configRoot;
            Logger = logger;
        }

        public string GameRootPath { get; private set; }
        public string LoaderPath { get; private set; }
        public string ModsPath { get; private set; }
        public IModLogger Logger { get; private set; }

        public string GetConfigPath(string fileName)
        {
            if (!Directory.Exists(_configRoot))
            {
                Directory.CreateDirectory(_configRoot);
            }

            return Path.Combine(_configRoot, fileName);
        }
    }
}
