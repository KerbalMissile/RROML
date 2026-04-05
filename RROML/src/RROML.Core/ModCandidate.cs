namespace RROML.Core
{
    internal sealed class ModCandidate
    {
        public string ModRoot { get; set; }
        public string AssemblyPath { get; set; }
        public ModManifest Manifest { get; set; }
    }
}
