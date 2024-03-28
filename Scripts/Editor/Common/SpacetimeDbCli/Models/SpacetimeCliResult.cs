using System.Collections.Generic;

namespace SpacetimeDB.Editor
{
    /// Result from SpacetimeDbCli.runCliCommandAsync
    public class SpacetimeCliResult
    {
        /// Raw, unparsed CLI output
        public string CliOutput { get; }
        
        /// This is the official error thrown by the CLI; it may not necessarily
        /// be as helpful as a friendlier error message likely within CliOutput.
        /// (!) Sometimes, this may not even be a "real" error. Double check output!
        public string CliError { get; }
        
        public List<string> ErrsFoundFromCliOutput { get; }
        public bool HasErrsFoundFromCliOutput => 
            ErrsFoundFromCliOutput?.Count > 0;
        
        /// Did we pass a CancellationToken and cancel the operation?
        public bool Cancelled { get; private set; }
        
        /// (!) While this may be a CLI error, it could be a false positive
        /// for what you really want to do. For example, `spacetime publish`
        /// will succeed, but throw a CliError for `wasm-opt` not found (unoptimized build).
        public bool HasCliErr => 
            !string.IsNullOrWhiteSpace(CliError) ||
            ErrsFoundFromCliOutput?.Count > 0;
        
        public SpacetimeCliResult(string cliOutput, string cliError)
        {
            // To prevent strange log formatting when paths are present, we replace `\` with `/`
            this.CliOutput = cliOutput?.Replace("\\", "/");
            this.CliError = cliError?.Replace("\\", "/");
            
            this.ErrsFoundFromCliOutput = getErrsFoundFromCliOutput();

            if (CliError == "Canceled")
                this.Cancelled = true;
        }

        private List<string> getErrsFoundFromCliOutput()
        {
            List<string> errsFound = new();
            string[] lines = CliOutput.Split('\n');
            
            foreach (string line in lines)
            {
                bool foundErr = line.Contains(": error CS");
                if (foundErr)
                {
                    errsFound.Add(line);
                }
            }
            return errsFound;
        }

        public SpacetimeCliResult(SpacetimeCliResult cliResult)
        {
            this.CliOutput = cliResult.CliOutput;
            
            // To prevent strange log formatting when paths are present, we replace `\` with `/`
            this.CliError = cliResult.CliError?.Replace("\\", "/");

            if (CliError == "Canceled")
                this.Cancelled = true;
        }
    }
}
