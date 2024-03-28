namespace SpacetimeDB.Editor
{
    /// Extends SpacetimeCliResult to catch specific `npm i -g wasm-opt` results
    public class InstallWasmResult : SpacetimeCliResult
    {
        /// Detects false-positive CliError:
        /// Success if CliOutput "changed {x} packages in {y}s"
        public bool IsSuccessfulInstall { get; }

        public InstallWasmResult(SpacetimeCliResult cliResult)
            : base(cliResult)
        {
            this.IsSuccessfulInstall = cliResult.CliOutput
                .TrimStart()
                .StartsWith("changed ");
        }
    }
}