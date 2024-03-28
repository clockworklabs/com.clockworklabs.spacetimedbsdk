namespace SpacetimeDB.Editor
{
    /// Extends SpacetimeCliResult to catch specific `spacetime publish` results
    public class GenerateResult : SpacetimeCliResult
    {
        /// Detects false-positive CliError: Success if CliOutput "Generate finished successfully"
        public bool IsSuccessfulGenerate { get; }

        public GenerateResult(SpacetimeCliResult cliResult)
            : base(cliResult)
        {
            this.IsSuccessfulGenerate = cliResult.CliOutput
                .Contains("Generate finished successfully");
        }
    }
}