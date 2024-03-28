using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpacetimeDB.Editor
{
    /// Result of `spacetime describe {identity}`
    /// (!) This only shows the db_addresses (hash); not nicknames.
    public class GetDbAddressesResult : SpacetimeCliResult
    {
        public List<string> DbAddresses { get; }
        public bool HasDbAddresses => DbAddresses?.Count > 0;
        
        
        public GetDbAddressesResult(SpacetimeCliResult cliResult)
            : base(cliResult.CliOutput, cliResult.CliError)
        {
            // Example raw list result below, where {identity} is replaced.
            // ###########################################################################################
            /*
                Associated database addresses for {identity}:

                 db_address
                ----------------------------------
                 7028275a6501ad4d87af00beedc1f531
                 59d1b0b1648398ac5d4f0319cb599382
                 f99ab6d262916b5f5fc0c35c126acdda
            */
            // ###########################################################################################
            
            // Initialize the list to store addresses
            this.DbAddresses = new List<string>();
            
            // Split the input string into lines considering the escaped newline characters
            string[] lines = CliOutput.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries); 

            // Corrected regex pattern to ensure it captures the nickname following the hash and spaces
            // This pattern assumes the nickname is the last element in the line after the hash
            const string pattern = @"\b[a-fA-F0-9]{32}\b";

            foreach (string line in lines)
            {
                Match match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    // Add the matched hash to the list
                    this.DbAddresses.Add(match.Value);
                }
            }
        }
    }
}