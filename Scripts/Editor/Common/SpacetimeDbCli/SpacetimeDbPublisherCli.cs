using System.Threading;
using System.Threading.Tasks;

namespace SpacetimeDB.Editor
{
    /// CLI action middleware between PublisherWindow and SpacetimeDbCli 
    /// Vanilla: Do the action -> return the result -> no more.
    public static class SpacetimeDbPublisherCli
    {
        #region High Level CLI Actions
        /// Publishes your SpacetimeDB server module
        /// Uses the `spacetime publish` CLI command, appending +args from UI elements
        public static async Task<PublishResult> PublishAsync(
            PublishRequest publishRequest,
            CancellationToken cancelToken)
        {
            string argSuffix = $"spacetime publish {publishRequest}";
            SpacetimeCliResult cliResult = await SpacetimeDbCli.runCliCommandAsync(argSuffix, cancelToken);
            PublishResult publishResult = new(publishRequest, cliResult);
            return publishResult;
        }
        
        /// Uses the `npm install -g wasm-opt` CLI command
        /// Success results from !CliError and "changed {numPkgs} packages in {numSecs}s" output
        public static async Task<InstallWasmResult> InstallWasmOptPkgAsync()
        {
            const string argSuffix = "npm install -g wasm-opt";
            SpacetimeCliResult cliResult = await SpacetimeDbCli.runCliCommandAsync(argSuffix);
            InstallWasmResult installWasmResult = new(cliResult);
            return installWasmResult;
        }
        
        /// Uses the `spacetime identity new` CLI command, then set as default.
        public static async Task<AddIdentityResult> AddIdentityAsync(AddIdentityRequest addIdentityRequest)
        {
            string argSuffix = $"spacetime identity new {addIdentityRequest}"; // Forced set as default
            SpacetimeCliResult cliResult = await SpacetimeDbCli.runCliCommandAsync(argSuffix);
            AddIdentityResult addIdentityResult = new(cliResult);
            return addIdentityResult;
        }
        
        /// Uses the `spacetime server add` CLI command, then set as default.
        public static async Task<AddServerResult> AddServerAsync(AddServerRequest addServerRequest)
        {
            // Forced set as default. Forced --no-fingerprint for local.
            string argSuffix = $"spacetime server add {addServerRequest}";
            SpacetimeCliResult cliResult = await SpacetimeDbCli.runCliCommandAsync(argSuffix);
            AddServerResult addServerResult = new(cliResult);
            return addServerResult;
        }
        
        /// Uses the `spacetime identity set-default` CLI command
        public static async Task<SpacetimeCliResult> SetDefaultIdentityAsync(string identityNicknameOrDbAddress)
        {
            string argSuffix = $"spacetime identity set-default {identityNicknameOrDbAddress}";
            SpacetimeCliResult cliResult = await SpacetimeDbCli.runCliCommandAsync(argSuffix);
            return cliResult;
        } 

        /// Uses the `spacetime server new` CLI command
        public static async Task<SpacetimeCliResult> SetDefaultServerAsync(string serverNicknameOrHost)
        {
            string argSuffix = $"spacetime server set-default {serverNicknameOrHost}";
            SpacetimeCliResult cliResult = await SpacetimeDbCli.runCliCommandAsync(argSuffix);
            return cliResult;
        }
        
        /// Uses the `spacetime generate` CLI command
        public static async Task<GenerateResult> GenerateClientFilesAsync(
            GenerateRequest generateRequest)
        {
            string argSuffix = $"spacetime generate {generateRequest}";
            SpacetimeCliResult cliResult = await SpacetimeDbCli.runCliCommandAsync(argSuffix);
            GenerateResult generateResult = new(cliResult);
            return generateResult;
        }
        #endregion // High Level CLI Actions
    }
}