namespace SpacetimeDB.Editor
{
    /// SpacetimeDB CLI Server { Nickname, Host, IsDefault }
    public class SpacetimeServer
    {
        /// Usage: "My-Case-InSeNsItIvE-Nickname" (underscores are also ok)
        public string Nickname { get; private set; }
        
        /// Usage: "https://127.0.0.1:3000" || "https://testnet.spacetimedb.com"
        /// (!) "localhost" won't work
        public string Host { get; private set; }
        public bool IsDefault { get; private set; }
        
        /// Starts with "https"
        public bool HasSsl { get; private set; }
        
        /// Example: "MyServer@http://localhost:3000 (isDefault? True)"
        public override string ToString() => $"{Nickname}@{Host} (isDefault? {IsDefault})";
        

        public SpacetimeServer(
            string nickname, 
            string host,
            bool isDefault = false)
        {
            this.Nickname = nickname;
            this.Host = host;
            this.HasSsl = host.StartsWith("https");
            this.IsDefault = isDefault;
        }
    }
}
