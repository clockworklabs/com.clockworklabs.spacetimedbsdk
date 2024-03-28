using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace SpacetimeDB.Editor
{
    /// Common CLI action helper for UI Builder windows.
    /// Vanilla: Do the action -> return the result -> no more.
    /// (!) Looking for more actions? See `SpacetimeDbPublisherCli.cs`
    public static class SpacetimeDbCli
    {
        #region Static Options
        private const CliLogLevel CLI_LOG_LEVEL = CliLogLevel.Info;
        
        public enum CliLogLevel
        {
            Info,
            Error,
        }
        #endregion // Static Options

        
        #region Init
        /// Install the SpacetimeDB CLI | https://spacetimedb.com/install 
        public static async Task<SpacetimeCliResult> InstallSpacetimeCliAsync()
        {
            if (CLI_LOG_LEVEL == CliLogLevel.Info)
                Debug.Log("Installing SpacetimeDB CLI tool...");
            
            SpacetimeCliResult result; 
            
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    result = await runCliCommandAsync("powershell -Command \"iwr " +
                        "https://windows.spacetimedb.com -UseBasicParsing | iex\"\n");
                    break;
                
                case RuntimePlatform.OSXEditor:
                    result = await runCliCommandAsync("brew install clockworklabs/tap/spacetime");
                    break;
                
                case RuntimePlatform.LinuxEditor:
                    result = await runCliCommandAsync("curl -sSf https://install.spacetimedb.com | sh");
                    break;
                
                default:
                    throw new NotImplementedException("Unsupported OS");
            }
            
            if (CLI_LOG_LEVEL == CliLogLevel.Info)
                Debug.Log($"Installed spacetimeDB CLI tool | {PublisherMeta.DOCS_URL}");
            
            return result;
        }
        #endregion // Init
        
        
        #region Core CLI
        /// Issue a cross-platform CLI cmd, where we'll start with terminal prefixes
        /// as the CLI "command" and some arg prefixes for compatibility.
        /// Usage: Pass an argSuffix, such as "spacetime version",
        ///        along with an optional cancel token
        public static async Task<SpacetimeCliResult> runCliCommandAsync(
            string argSuffix, 
            CancellationToken cancelToken = default)
        {
            string output = string.Empty;
            string error = string.Empty;
            Process process = new();
            CancellationTokenRegistration cancellationRegistration = default;

            try
            {
                string terminal = getTerminalPrefix(); // Determine terminal based on platform
                string argPrefix = getCommandPrefix(); // Determine command prefix (cmd /c, etc.)
                string fullParsedArgs = $"{argPrefix} \"{argSuffix}\"";

                process.StartInfo.FileName = terminal;
                process.StartInfo.Arguments = fullParsedArgs;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                
                // Input Logs
                if (CLI_LOG_LEVEL == CliLogLevel.Info)
                {
                    Debug.Log("CLI Input: \n```\n<color=yellow>" +
                        $"{terminal} {fullParsedArgs}</color>\n```\n");
                }

                process.Start();

                // Register cancellation token to safely handle process termination
                cancellationRegistration = cancelToken.Register(() => terminateProcessSafely(process));

                // Asynchronously read output and error
                Task<string> readOutputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> readErrorTask = process.StandardError.ReadToEndAsync();

                // Wait for the process to exit or be cancelled
                while (!process.HasExited)
                    await Task.Delay(100, cancelToken);

                // Await the read tasks to ensure output and error are captured
                output = await readOutputTask;
                error = await readErrorTask;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("CLI Warning: Canceled");
                error = "Canceled";
            }
            catch (Exception e)
            {
            }
            finally
            {
                // Heavy cleanup
                await cancellationRegistration.DisposeAsync();
                if (!process.HasExited)
                    process.Kill();
                process.Dispose(); // No async ver for this Dispose
            }
            
            // Process results, log err (if any), return parsed Result 
            SpacetimeCliResult cliResult = new(output, error);
            logCliResults(cliResult);

            return new SpacetimeCliResult(output, error);
        }

        public static void terminateProcessSafely(Process process)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                    if (!process.WaitForExit(5000)) // Wait up to 5 seconds
                        process.Kill(); // Force terminate the process if it hasn't exited
                }
            }
            catch (InvalidOperationException e)
            {
                // Process likely already exited or been disposed; safe to ignore, in most cases
                Debug.LogWarning($"Attempted to terminate a process: {e.Message}");
            }
        }

        public static void logCliResults(SpacetimeCliResult cliResult)
        {
            bool hasOutput = !string.IsNullOrEmpty(cliResult.CliOutput);
            bool hasLogLevelInfoNoErr = CLI_LOG_LEVEL == CliLogLevel.Info && !cliResult.HasCliErr;
            string prettyOutput = $"\n```\n<color=yellow>{cliResult.CliOutput}</color>\n```\n";

            if (hasOutput && hasLogLevelInfoNoErr)
            {
                Debug.Log($"CLI Output: {prettyOutput}");
            }

            if (cliResult.HasCliErr)
            {
                // There may be only a CliError and no CliOutput, depending on the type of error.
                if (!string.IsNullOrEmpty(cliResult.CliOutput))
                {
                    Debug.Log($"CLI Output: {prettyOutput}");
                }
                
                Debug.LogError($"CLI Error: {cliResult.CliError}\n" +
                    "(For +details, see output err above)");

                // Separate the errs found from the CLI output so the user doesn't need to dig
                bool logCliResultErrsSeparately = cliResult.ErrsFoundFromCliOutput?.Count is > 0 and < 5;
                
                if (cliResult.HasErrsFoundFromCliOutput & logCliResultErrsSeparately) // If not too many
                {
                    for (int i = 0; i < cliResult.ErrsFoundFromCliOutput.Count; i++)
                    {
                        string err = cliResult.ErrsFoundFromCliOutput[i];
                        Debug.LogError($"CLI Error Summary[{i}]: {err}");
                    }
                }
            }
        }
        
        public static string getCommandPrefix()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return "/c";
                
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                    return "-c";
                
                default:
                    Debug.LogError("Unsupported OS");
                    return null;
            }
        }

        /// Return either "cmd.exe" || "/bin/bash"
        public static string getTerminalPrefix()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return "cmd.exe";
                
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                    return "/bin/bash";
                
                default:
                    Debug.LogError("Unsupported OS");
                    return null;
            }
        }
        #endregion // Core CLI
            
        
        #region High Level CLI Actions
        /// isInstalled = !cliResult.HasCliError 
        public static async Task<SpacetimeCliResult> GetIsSpacetimeCliInstalledAsync()
        {
            string argSuffix = "spacetime version";
            SpacetimeCliResult cliResult = await runCliCommandAsync(argSuffix);
            return cliResult;
        }
        
        /// Uses the `spacetime identity list` CLI command
        public static async Task<GetIdentitiesResult> GetIdentitiesAsync()
        {
            string argSuffix = "spacetime identity list";
            SpacetimeCliResult cliResult = await runCliCommandAsync(argSuffix);
            GetIdentitiesResult getIdentitiesResult = new(cliResult);
            return getIdentitiesResult;
        }
        
        /// Uses the `spacetime identity list` CLI command
        public static async Task<GetServersResult> GetServersAsync() 
        {
            string argSuffix = "spacetime server list";
            SpacetimeCliResult cliResult = await runCliCommandAsync(argSuffix);
            GetServersResult getServersResult = new(cliResult);
            return getServersResult;
        }
        #endregion // High Level CLI Actions


        /// Uses the `spacetime list {identity}` CLI command.
        /// (!) This only returns the addresses.
        ///     For nicknames, see the chained call: GetDbAddressesWithNicknames
        public static async Task<GetDbAddressesResult> GetDbAddresses(string identity)
        {
            string argSuffix = $"spacetime list {identity}";
            SpacetimeCliResult cliResult = await runCliCommandAsync(argSuffix);
            GetDbAddressesResult getDbAddressesResult = new(cliResult);
            return getDbAddressesResult;
        }
        
        /// [Slow] Uses the `spacetime describe {moduleName} [--as-identity {identity}]` CLI command
        public static async Task<GetEntityStructureResult> GetEntityStructure(
            string moduleName,
            string asIdentity = null)
        {
            // Append ` --as-identity {identity}`?
            string asIdentitySuffix = string.IsNullOrEmpty(asIdentity) ? "" : $" --as-identity {asIdentity}";
            string argSuffix = $"spacetime describe {moduleName}{asIdentitySuffix}";
            
            SpacetimeCliResult cliResult = await runCliCommandAsync(argSuffix);
            GetEntityStructureResult getEntityStructureResult = new(cliResult);
            return getEntityStructureResult;
        }
    }
}