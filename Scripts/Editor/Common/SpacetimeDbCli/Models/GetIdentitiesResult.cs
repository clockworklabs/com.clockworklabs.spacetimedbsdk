using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpacetimeDB.Editor
{
    /// Result of `spacetime identity list`
    public class GetIdentitiesResult : SpacetimeCliResult
    {
        public List<SpacetimeIdentity> Identities { get; private set; }
        public bool HasIdentity => Identities?.Count > 0;
        public bool HasIdentitiesButNoDefault => HasIdentity && 
            !Identities.Exists(id => id.IsDefault);
        
        
        public GetIdentitiesResult(SpacetimeCliResult cliResult)
            : base(cliResult.CliOutput, cliResult.CliError)
        {
            // Example raw list result below. Notice how the top hash has no associated Nickname.
            // ###########################################################################################
            /*
             DEFAULT  IDENTITY                                                          NAME            
                      1111111111111111111111111111111111111111111111111111111111111111                  
                      2222222222222222222222222222222222222222222222222222222222222222  Nickname2
                      3333333333333333333333333333333333333333333333333333333333333333  Nickname3       
             */
            // ###########################################################################################
            
            // Initialize the list to store nicknames
            this.Identities = new List<SpacetimeIdentity>();
            
            // Split the input string into lines considering the escaped newline characters
            string[] lines = CliOutput.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries); 
            const string pattern = @"(?:\*\*\*\s+)?\b[a-fA-F0-9]{64}\s+(.+)$"; // Captures nicknames

            foreach (string line in lines)
            {
                Match match = Regex.Match(line, pattern);
                if (!match.Success || match.Groups.Count <= 1)
                    continue;
                
                // Extract potential match
                string potentialNickname = match.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(potentialNickname))
                    onIdentityFound(line, potentialNickname);
            }
        }
        
        /// Set identityNicknames and isDefault
        private void onIdentityFound(string line, string nickname)
        {
            // Determine if the newIdentity is marked as default by checking if the line contains ***
            bool isDefault = line.Contains("***");
            SpacetimeIdentity identity = new(nickname, isDefault);
            Identities.Add(identity);
        }
    }
}