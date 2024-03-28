namespace SpacetimeDB.Editor
{
    /// Info passed from the UI to CLI during the CLI `spacetime server new`
    /// Print ToString to get the CLI `"{Host}" "{Nickname}" -d`
    public class AddServerRequest : SpacetimeServer
    {
        private readonly bool IsLocal;

        /// Example: `"https://testnet.spacetimedb.com" testnet -d`
        /// Adding a new server will *always* set it as default
        /// Local will add `--no-fingerprint` so it doesn't need to be running
        public override string ToString() =>
            $"-d {(IsLocal ? "--no-fingerprint" : "")} " +
            $"\"{Host}\" {Nickname}";
        
        
        public AddServerRequest(string nickname, string host)
            : base(nickname, host, isDefault: true)
        {
            this.IsLocal = host.Contains("127.0.0.1");
        }
    }
}
