using System.Collections.Generic;

namespace SpacetimeDB.Editor
{
    public class GetDbAddressesWithNicknamesResult : GetDbAddressesResult
    {
        public List<string> DbNicknames { get; }
        
        
        public GetDbAddressesWithNicknamesResult(SpacetimeCliResult cliResult, List<string> dbNicknames)
            : base(cliResult)
        {
            this.DbNicknames = dbNicknames;
        }
    }
}