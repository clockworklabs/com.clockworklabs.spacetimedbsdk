namespace SpacetimeDB.Editor
{
    /// Info passed from the UI to CLI during the CLI `spacetime identity new`
    /// Print ToString to get the CLI "-d --name {nickname} --email {email}"
    /// Forces default (-d).
    public class AddIdentityRequest : SpacetimeIdentity
    {
        /// Usage: "a@b.c" || "a+1@b.c"
        public string Email { get; private set; }
        
        /// Returns what's sent to the CLI: "-d --name {nickname} --email {email}"
        public override string ToString() => 
            $"-d --name \"{Nickname}\" --email \"{Email}\"";
        

        /// Sets nickname + email. Forces default.
        public AddIdentityRequest(
            string nickname, 
            string email)
            : base(nickname, isDefault:true)
        {
            this.Email = email;
        }
    }
}