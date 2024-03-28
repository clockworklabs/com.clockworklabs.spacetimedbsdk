using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpacetimeDB.Editor
{
    /// Result of `spacetime server list`
    public class GetServersResult : SpacetimeCliResult
    {
        public List<SpacetimeServer> Servers { get; private set; }
        public bool HasServer => Servers?.Count > 0;
        public bool HasServersButNoDefault => HasServer && 
            !Servers.Exists(id => id.IsDefault);
        
        
        public GetServersResult(SpacetimeCliResult cliResult)
            : base(cliResult.CliOutput, cliResult.CliError)
        {
            // Example raw servers below. Notice default contains "***".
            /*
            // ###########################################################################################
             DEFAULT  HOSTNAME                 PROTOCOL  NICKNAME
                 ***  testnet.spacetimedb.com  https     testnet
                      127.0.0.1:3000           http      local
            // ###########################################################################################
             */
            
            // Initialize the list to store nicknames
            this.Servers = new List<SpacetimeServer>();
            
            // Split the input string into lines considering the escaped newline characters
            string[] lines = CliOutput.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries); 

            // Captures hostName, protocol (ignored in this context), nickname, and isDefault
            const string pattern = @"^\s*(\*{3})?\s*([^\s]+)\s+[^\s]+\s+([^\s]+)";

            foreach (string line in lines)
            {
                Match match = Regex.Match(line, pattern);
                bool isHeader = match.Groups[3].Value == "PROTOCOL";
                if (!match.Success || isHeader)
                    continue;
                
                SpacetimeServer server = new(
                    nickname: match.Groups[3].Value,
                    host: match.Groups[2].Value,
                    isDefault: match.Groups[1].Value == "***");

                Servers.Add(server);
            }
        }
    }
}