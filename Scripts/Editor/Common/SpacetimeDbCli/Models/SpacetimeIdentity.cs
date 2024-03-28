namespace SpacetimeDB.Editor
{
    /// SpacetimeDB CLI Identity { Nickname, IsDefault }
    public class SpacetimeIdentity
    {
        /// Usage: "My-Case-InSeNsItIvE-Nickname" (underscores are also ok)
        public string Nickname { get; private set; }

        public bool IsDefault { get; private set; }
        
        public override string ToString() => $"{Nickname} (isDefault? {IsDefault})";
        

        public SpacetimeIdentity(
            string nickname, 
            bool isDefault = false)
        {
            this.Nickname = nickname;
            this.IsDefault = isDefault;
        }
    }
}
