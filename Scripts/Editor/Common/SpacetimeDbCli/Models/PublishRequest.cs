namespace SpacetimeDB.Editor
{
    /// Info passed from the UI to CLI during the CLI `spacetime publish`
    /// Print ToString to get the CLI "--project-path {path} {module-name}"
    public class PublishRequest
    {
        public class AdvancedOpts
        {
            /// When true, appends --clear-database to clear the db data
            public bool ClearDbData { get; }
            
            /// When true, appends --debug and --skip_clippy 
            public bool IsDebugMode { get; }
            
            public AdvancedOpts(bool clearDbData, bool isDebugMode)
            {
                this.ClearDbData = clearDbData;
                this.IsDebugMode = isDebugMode;
            }
            
            public AdvancedOpts(AdvancedOpts advancedOpts)
            {
                this.ClearDbData = advancedOpts.ClearDbData;
                this.IsDebugMode = advancedOpts.IsDebugMode;
            }
        }
        
        public AdvancedOpts Advanced { get; }
        
        /// Usage: "my-server-module-name"
        public string ServerModuleName { get; }

        /// Usage: "absolute/path/to/server/module/dir"
        public string ServerModulePath { get; }
        
        /// Returns what's sent to the CLI: "{clearDbStr}--project-path {path} {module-name}"
        public override string ToString()
        {
            string clearDbDataStr = Advanced.ClearDbData ? "--clear-database " : "";
            string debugModeStr = Advanced.IsDebugMode ? "--debug --skip_clippy " : "";
            return $"{clearDbDataStr}{debugModeStr}--project-path \"{ServerModulePath}\" {ServerModuleName}";
        }
        

        public PublishRequest(
            string serverModuleName, 
            string serverModulePath,
            AdvancedOpts advancedOpts)
        {
            this.ServerModuleName = serverModuleName;
            this.ServerModulePath = serverModulePath;
            this.Advanced = new AdvancedOpts(advancedOpts);
        }
    }
}