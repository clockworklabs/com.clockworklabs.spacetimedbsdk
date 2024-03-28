using System.Threading.Tasks;

namespace SpacetimeDB.Editor
{
    /// CLI action middleware between ReducerWindow and SpacetimeDbCli 
    /// Vanilla: Do the action -> return the result -> no more.
    /// (!) Didn't find what you were looking for here? Check `SpacetimeDbCli.cs`
    public static class SpacetimeDbReducerCli
    {
        #region High Level CLI Actions
        /// Uses the `spacetime call` CLI command
        public static async Task<SpacetimeCliResult> CallReducerAsync(CallReducerRequest request)
        {
            string argSuffix = $"spacetime call {request}";
            SpacetimeCliResult cliResult = await SpacetimeDbCli.runCliCommandAsync(argSuffix);
            return cliResult;
        }
        #endregion // High Level CLI Actions
    }
}