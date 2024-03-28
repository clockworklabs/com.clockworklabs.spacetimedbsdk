using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace SpacetimeDB.Editor
{
    /// Validations, trimming, special formatting
    public partial class PublisherWindow 
    {
        private static string replaceSpacesWithDashes(string str) =>
            str?.Replace(" ", "-");
        
        /// Remove ALL whitespace from string
        private static string superTrim(string str) =>
            str?.Replace(" ", "");

        /// This checks for valid email chars for OnChange events
        private static bool tryFormatAsEmail(string input, out string formattedEmail)
        {
            formattedEmail = null;
            if (string.IsNullOrWhiteSpace(input)) 
                return false;
    
            // Simplified regex pattern to allow characters typically found in emails
            const string emailCharPattern = @"^[a-zA-Z0-9@._+-]+$"; // Allowing "+" (email aliases)
            if (!Regex.IsMatch(input, emailCharPattern))
                return false;
    
            formattedEmail = input;
            return true;
        }

        /// Useful for FocusOut events, checking the entire email for being valid.
        /// At minimum: "a@b.c"
        private static bool checkIsValidEmail(string emailStr)
        {
            // No whitespace, contains "@" contains ".", allows "+" (alias), contains chars in between
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(emailStr, pattern);
        }

        /// Useful for FocusOut events, checking the entire host for being valid.
        /// At minimum, must start with "http".
        private static bool checkIsValidUrl(string url) => url.StartsWith("http");

        /// Checked at OnFocusOut events to ensure both nickname+email txt fields are valid.
        /// Toggle identityAddBtn enabled based validity of both.
        private void checkIdentityReqsToggleIdentityBtn()
        {
            bool isNicknameValid = !string.IsNullOrWhiteSpace(identityNicknameTxt.value);
            bool isEmailValid = checkIsValidEmail(identityEmailTxt.value);
            identityAddBtn.SetEnabled(isNicknameValid && isEmailValid);
        }
        
        /// Checked at OnFocusOut events to ensure both nickname-host txt fields are valid.
        /// Toggle serverAddBtn enabled based validity of both.
        private void checkServerReqsToggleServerBtn()
        {
            bool isHostValid = checkIsValidUrl(serverHostTxt.value);
            bool isNicknameValid = !string.IsNullOrWhiteSpace(serverNicknameTxt.value);
            serverAddBtn.SetEnabled(isNicknameValid && isHostValid);
        }
        
        private void resetCancellationTokenSrc()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        /// <returns>
        /// dashified-project-name, suggested based on your project name.
        /// Swaps out `client` keyword with `server`.</returns>
        private string getSuggestedServerModuleName()
        {
            // Prefix "unity-", dashify the name, replace "client" with "server (if found).
            // Use Unity's productName
            string unityProjectName = $"unity-{Application.productName.ToLowerInvariant()}";
            string projectNameDashed = Regex
                .Replace(unityProjectName, @"[^a-z0-9]", "-")
                .Replace("client", "server");

            return projectNameDashed;
        }
    }
}