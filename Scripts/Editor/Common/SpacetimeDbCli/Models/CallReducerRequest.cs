namespace SpacetimeDB.Editor
{
    /// Info passed from the UI to CLI during the CLI `spacetime call`
    /// Print ToString to get the CLI "{moduleName} {reducerName}[ {args}]"
    public class CallReducerRequest
    {
        public string ModuleName { get; }
        public string ReducerName { get; }
        
        /// Appends --as-identity {identity}
        /// (name || address)
        public string CallAsAltIdentity { get; }
        
        /// Optional, implicit args to append
        public string Args { get; }

        /// <summary>Returns what's sent to the CLI after the initial command</summary>
        /// <returns>"{moduleName} {reducerName}[ {callAsAltIdentity}][ {args}]"</returns>
        public override string ToString()
        {
            bool hasAltIdentity = !string.IsNullOrEmpty(CallAsAltIdentity);
            string callAsAltIdentity = hasAltIdentity ? $" --as-identity {CallAsAltIdentity}" : "";
            
            bool hasArgs = !string.IsNullOrEmpty(Args);
            string cliArgs = hasArgs ? $" {Args}" : "";

            return $"\"{ModuleName}\" \"{ReducerName}\"{callAsAltIdentity}{cliArgs}";
        }

        /// Sets ModuleName + ReducerName [+ Args]
        public CallReducerRequest(
            string moduleName, 
            string reducerName,
            string callAsAltIdentity = null,
            string args = "")
        {
            this.ModuleName = moduleName;
            this.ReducerName = reducerName;
            this.CallAsAltIdentity = callAsAltIdentity;
            this.Args = args;
        }
    }
}
