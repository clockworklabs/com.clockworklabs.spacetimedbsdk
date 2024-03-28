using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

namespace SpacetimeDB.Editor
{
    /// Extends SpacetimeCliResult to catch specific `spacetime publish` results
    public class PublishResult : SpacetimeCliResult
    {
        /// Still contains useful information to use later, such as path to module
        public PublishRequest Request { get; }
        
        #region Success
        /// The errors may have false-positive warnings; this is the true success checker
        public bool IsSuccessfulPublish { get; }
        
        public DateTime PublishedAt { get; private set; } 
        
        /// Warning: `wasm-opt` !found, so the module continued with an "unoptimised" version
        public bool IsPublishWasmOptimized { get; private set; }

        /// Eg: "http://localhost:3000" || "https://testnet.spacetimedb.com"
        public string UploadedToHost { get; private set; }

        /// Eg: "http://localhost"
        public string UploadedToUrl { get; private set; }

        /// Eg: 3000
        public ushort UploadedToPort { get; private set; }

        /// Not to be confused with UploadedToHost. Eg: "123abc123abc123abc123abc123abc12"
        public string DatabaseAddressHash { get; private set; }

        public bool IsLocal { get; private set; }
        
        public DbPublishType PublishType { get; private set; }

        public enum DbPublishType
        {
            Unknown,
            Created,
            Updated,
        }
        #endregion // Success


        #region Errs
        /// Parsed from known MSBUILD publishing error codes
        /// (!) We currently only catch invalid project dir errors
        public PublishErrorCode PublishErrCode { get; private set; }
        
        /// You may pass this raw string to the UI, if a known err is caught
        public string StyledFriendlyErrorMessage { get; private set; }
        
        /// (!) These may be lesser warnings. For a true indicator of success, check `IsSuccessfulPublish` 
        public bool HasPublishErr => PublishErrCode != PublishErrorCode.None;
        
        public enum PublishErrorCode
        {
            None,
            MSB1003_InvalidProjectDir,
            OS10061_ServerHostNotRunning,
            DBUpdateRejected_PermissionDenied,
            CancelledOperation,
            UnknownError,
        }
        #endregion // Errs


        public PublishResult(PublishRequest request, SpacetimeCliResult cliResult)
            : base(cliResult.CliOutput, cliResult.CliError)
        {
            this.Request = request;
            bool hasOutputErr = CliOutput.Contains("Error:");

            if (cliResult.HasCliErr || hasOutputErr)
            {
                if (cliResult.HasCliErr)
                    onCliError(cliResult);
            
                // CLI resulted success, but what about an internal error specific to publisher?
                if (cliResult.CliError.Contains("Error:"))
                    onPublisherError(cliResult);
            }
            
            // "created new database with domain" || "updated database with domain"
            this.IsSuccessfulPublish = CliOutput.Contains("database with domain");
            if (!IsSuccessfulPublish)
                return;

            onSuccess();
        }

        private void onSuccess()
        {
            this.PublishedAt = DateTime.Now;
            this.IsPublishWasmOptimized = !CliError.Contains("Could not find wasm-opt");
            this.IsLocal = CliOutput.Contains("Uploading to local =>");
            this.DatabaseAddressHash = getDatabaseAddressHash();
            setUploadedToInfo();
        }

        private void setUploadedToInfo()
        {// Use regex to find the host url from CliOutput.
            // Eg, from "Uploading to local => http://127.0.0.1:3000"
            // Eg, from "Uploading to testnet => https://testnet.spacetimedb.com"
            (string url, string port)? urlPortTuple = getHostUrlFromCliOutput();
            if (urlPortTuple == null)
                return;

            this.UploadedToHost = $"{urlPortTuple.Value.url}:{urlPortTuple.Value.port}";
            this.UploadedToUrl = urlPortTuple.Value.url;
            
            // There may not be a port
            if (ushort.TryParse(urlPortTuple.Value.port, out ushort parsedPort))
            {
                this.UploadedToPort = parsedPort; // Parsing successful, update with the parsed value
                return;
            }
            
            // No port - assume based on http(s) prefix
            this.UploadedToPort = UploadedToHost.StartsWith("https") 
                ? (ushort)443 // ssl
                : (ushort)80; // !ssl
            
            // We also need to remove the `:` from the url host
            this.UploadedToHost = UploadedToHost.Replace(":", "");
        }

        private void onPublisherError(SpacetimeCliResult cliResult)
        {
            bool isServerNotRunning = cliResult.CliError.Contains("os error 10061");
            bool isErrBuriedInOutput = !isServerNotRunning && cliResult.HasErrsFoundFromCliOutput;
                
            if (isServerNotRunning)
            {
                this.PublishErrCode = PublishErrorCode.OS10061_ServerHostNotRunning;
                this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                    SpacetimeMeta.StringStyle.Error,
                    "<b>Failed to Publish:</b> Server host not running\n<align=left>" +
                    "(1) Open terminal\n" +
                    "(2) Enter command: `<color=white>spacetime start</color>`\n" +
                    "(3) Try again");
            }
            else if (isErrBuriedInOutput)
            {
                this.PublishErrCode = PublishErrorCode.UnknownError;

                string clippedIteratedErrs = string.Join("\n\n", cliResult.ErrsFoundFromCliOutput);
                clippedIteratedErrs = Utils.ClipString(clippedIteratedErrs, maxLength: 4000);

                this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                    SpacetimeMeta.StringStyle.Error,
                    $"<b>Failed to Publish:</b>\n{clippedIteratedErrs}");
            }
            else
            {
                this.PublishErrCode = PublishErrorCode.UnknownError;
            }
        }

        private void onCliError(SpacetimeCliResult cliResult)
        {
            // CliError >>
            bool isCancelled = cliResult.CliError == "Canceled";
            if (isCancelled)
            {
                this.PublishErrCode = PublishErrorCode.UnknownError;
                this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                    SpacetimeMeta.StringStyle.Error,
                    "Cancelled");
                return;
            }
            
            bool hasErrWorkingProjDirNotFound =
                cliResult.HasCliErr &&
                cliResult.CliOutput.Contains("error MSB1003"); // Working proj dir !found

            if (hasErrWorkingProjDirNotFound)
            {
                this.PublishErrCode = PublishErrorCode.MSB1003_InvalidProjectDir;
                this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                    SpacetimeMeta.StringStyle.Error,
                    "<b>Failed:</b> Invalid server module dir");
                return;
            }
                    
            // "Error: Database update rejected: Update database `{dbAddressHash}`: Permission denied"
            bool hasErrPermissionDenied = cliResult.CliError.Contains("Permission denied");
            if (hasErrPermissionDenied)
            {
                this.PublishErrCode = PublishErrorCode.DBUpdateRejected_PermissionDenied;
                this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                    SpacetimeMeta.StringStyle.Error,
                    "<b>Failed:</b> Database update rejected\n" +
                    "Permission denied - Identity<>Server<>Nickname mismatch.\n" +
                    "(Identites and Module Nicknames are bound per-server)");
                return;
            }
        }

        /// Use regex to find the host url from CliOutput.
        /// Eg, from "Uploading to local => http://127.0.0.1:3000"
        private (string url, string port)? getHostUrlFromCliOutput()
        {
            const string pattern = @"Uploading to .* => (https?://([^:/\s]+)(?::(\d+))?)";
            Match match = Regex.Match(CliOutput, pattern);

            if (!match.Success)
                return null;

            string url = match.Groups[2].Value;
            string port = match.Groups[3].Value; // Optional
            
            // url sanity check
            if (string.IsNullOrEmpty(url))
                Debug.LogError("Failed to parse host url from CliOutput");

            return (url, port);
        }

        /// This could either be from "created" or "updated" prompts.
        /// Eg: "123abc123abc123abc123abc123abc12"
        private string getDatabaseAddressHash()
        {
            // ###############################################################################################
            // Created new database with domain: unity-server, address: 123abc123abc123abc123abc123abc12
            // ###############################################################################################
            // Updated database with domain: unity-server, address: 123abc123abc123abc123abc123abc12
            // ###############################################################################################
            
            // We either updated or created
            const string commonPattern = @"database with domain:.+address: (\w+)";
            
            string updatedPattern = $@"Updated {commonPattern}";
            Match match = Regex.Match(CliOutput, updatedPattern);
            if (match.Success)
            {
                // Success - Updated
                this.PublishType = DbPublishType.Updated;
                return match.Groups[1].Value;
            }
            
            string createdPattern = $@"Created new {commonPattern}";
            match = Regex.Match(CliOutput, createdPattern);
            {
                // Success - Created
                this.PublishType = DbPublishType.Created;
                return match.Groups[1].Value;
            }
        }

        /// Returns a json summary
        public override string ToString() => 
            $"{nameof(PublishResult)}: " +
            $"{JsonConvert.SerializeObject(this, Formatting.Indented)}";
    }
}