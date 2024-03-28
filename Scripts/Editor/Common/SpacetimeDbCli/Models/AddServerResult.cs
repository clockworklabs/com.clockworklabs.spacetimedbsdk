namespace SpacetimeDB.Editor
{
    public class AddServerResult : SpacetimeCliResult
    {
        #region Errors
        /// Does an err exist?
        public bool HasAddServerError { get; private set; }
        
        /// You may pass this raw string to the UI, if a known err is caught.
        /// This err won't be friendly, if uncaught (we pass CliError).
        /// Clipped, if too long.
        public string StyledFriendlyErrorMessage { get; private set; }
        
        /// Known/caught error type
        public AddServerErrorType AddServerError { get; private set;}
        
        public enum AddServerErrorType
        {
            None,
            ServerNotRunning,
            NicknameAlreadyInUse,
            Unknown,
        }
        #endregion // Errors
        
        
        public AddServerResult(SpacetimeCliResult cliResult)
            : base(cliResult.CliOutput, cliResult.CliError)
        {
            // Catch known errors
            if (!HasCliErr)
                return;
            
            if (checkHasServerNotRunningError())
                return;

            if (checkIsNicknameAlreadyInUseError())
                return;
            
            // Unknown error
            this.AddServerError = AddServerErrorType.Unknown;
            
            string clippedFriendlyErr = Utils.ClipString(CliError, maxLength: 4000);
            this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error, 
                clippedFriendlyErr);
        }

        /// <returns>hasErr</returns>
        private bool checkIsNicknameAlreadyInUseError()
        {
            // Server nickname testnet already in use
            bool isNicknameInUseErr = HasCliErr && CliError.Contains("already in use");
            if (!isNicknameInUseErr)
                return false; // !hasErr
            
            this.HasAddServerError = true;
            this.AddServerError = AddServerErrorType.NicknameAlreadyInUse;
            this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error, 
                "Nickname already in use");
            return true; // hasErr
        }

        /// <returns>hasErr</returns>
        private bool checkHasServerNotRunningError()
        {
            bool hasServerNotRunningErr = HasCliErr && CliError.Contains("Is the server running?");
            if (!hasServerNotRunningErr)
                return false; // !hasErr
            
            this.HasAddServerError = true;
            this.AddServerError = AddServerErrorType.ServerNotRunning;
            this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error, 
                "Failed to handshake; is the server running?");
            return true; // hasErr
        }
    }
}