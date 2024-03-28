namespace SpacetimeDB.Editor
{
    /// Info passed from the UI to CLI during the CLI `spacetime call`
    /// Print ToString to get the CLI "{moduleName} {reducerName}[ {args}]"
    public class CallReducerRequest
    {
        public string ModuleName { get; }
        public string ReducerName { get; }
        
        /// Sometimes optional
        public string Args { get; }

        /// <summary>Returns what's sent to the CLI after the initial command</summary>
        /// <returns>"{moduleName} {reducerName}[ {args}]"</returns>
        public override string ToString()
        {
            bool hasArgs = !string.IsNullOrEmpty(Args);
            string cliArgs = hasArgs ? $" {Args}" : "";

            return $"\"{ModuleName}\" \"{ReducerName}\"{cliArgs}";
        }

        /// Sets ModuleName + ReducerName [+ Args]
        public CallReducerRequest(
            string moduleName, 
            string reducerName,
            string args = "")
        {
            this.ModuleName = moduleName;
            this.ReducerName = reducerName;
            this.Args = args;
        }
    }
}
