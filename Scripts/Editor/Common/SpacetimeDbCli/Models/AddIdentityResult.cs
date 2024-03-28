namespace SpacetimeDB.Editor
{
    /// Result from SpacetimeDbPublisherCli.AddIdentityAsync
    public class AddIdentityResult : SpacetimeCliResult
    {
        #region Errors
        /// Does an err exist?
        public bool HasAddIdentityError { get; private set; }
        
        /// You may pass this raw string to the UI, if a known err is caught.
        /// This err won't be friendly, if uncaught (we pass CliError)
        public string StyledFriendlyErrorMessage { get; private set; }
        
        /// Known/caught error type
        public AddIdentityErrorType AddIdentityError { get; private set;}
        
        public enum AddIdentityErrorType
        {
            None,
            IdentityAlreadyExists,
            TimedOut,
            Unknown,
        }
        #endregion // Errors
        
        
        public AddIdentityResult(SpacetimeCliResult cliResult)
            : base(cliResult.CliOutput, cliResult.CliError)
        {
            // Catch known errors
            if (!HasCliErr)
                return;
            
            if (checkIsIdentityAlreadyExistsError())
                return;
            if (checkIsTimedOutError())
                return;
            
            // Unknown error
            this.AddIdentityError = AddIdentityErrorType.Unknown;

            string clippedFriendlyErr = Utils.ClipString(CliError, maxLength: 4000);
            this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error, 
                clippedFriendlyErr);
        }

        private bool checkIsTimedOutError()
        {
            // Identity testnet already exists
            bool isTimedOutErr = HasCliErr && CliError.Contains("host has failed to respond");
            if (!isTimedOutErr)
                return false; // !hasErr
            
            this.HasAddIdentityError = true;
            this.AddIdentityError = AddIdentityErrorType.TimedOut;
            this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error, 
                "Timed out");
            return true; // hasErr
        }

        /// <returns>hasErr</returns>
        private bool checkIsIdentityAlreadyExistsError()
        {
            // Identity testnet already exists
            bool isIdentityAlreadyExistsErr = HasCliErr && CliError.Contains("that name already exists");
            if (!isIdentityAlreadyExistsErr)
                return false; // !hasErr
            
            this.HasAddIdentityError = true;
            this.AddIdentityError = AddIdentityErrorType.IdentityAlreadyExists;
            this.StyledFriendlyErrorMessage = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error, 
                "Nickname already exists");
            return true; // hasErr
        }
    }
}