namespace SpacetimeDB.Editor
{
    /// Info passed from the UI to CLI during the CLI `spacetime generate`
    /// Print ToString to get the CLI args
    public class GenerateRequest
    {
        /// Usage: "absolute/path/to/server/module/dir"
        public string ServerModulePath { get; }
        
        /// Recommended: $"{UnityEngine.Application.dataPath}/StdbAutogen"
        public string OutDir { get; }
        
        /// When true, appends `--delete-files`
        public bool DeleteOutdatedFiles { get; }

        /// Returns what's sent to the CLI: "[--delete-files] --lang csharp --project-path {path} --out-dir {path}"
        public override string ToString()
        {
            string deleteFiles = DeleteOutdatedFiles ? "--delete-files " : "";
            string projectPath = $"--project-path \"{ServerModulePath}\"";
            string outDir = $"--out-dir \"{OutDir}\"";

            return $"{deleteFiles}--lang csharp {projectPath} {outDir}";
        }
        

        public GenerateRequest(
            string serverModulePath, 
            string outDir,
            bool deleteOutdatedFiles)
        {
            this.ServerModulePath = serverModulePath;
            this.OutDir = outDir;
            this.DeleteOutdatedFiles = deleteOutdatedFiles;
        }
    }
}