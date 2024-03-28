/*  SpacetimeDB for Unity
 *  *  This class is only used in Unity projects. Attach this to a gameobject in your scene to use SpacetimeDB.
 *  *
 */
#if UNITY_5_3_OR_NEWER
using UnityEngine;

namespace SpacetimeDB
{
    // This is an optional helper class to store your auth token in PlayerPrefs
    // Override GetTokenKey() if you want to use a player pref key specific to your game
    public static class AuthToken
    {
        public static string Token => PlayerPrefs.GetString(GetTokenKey());

        public static void SaveToken(string token)
        {
            PlayerPrefs.SetString(GetTokenKey(), token);
        }

        public static string GetTokenKey()
        {
            var key = "spacetimedb.identity_token";
#if UNITY_EDITOR
            // Different editors need different keys
            key += $" - {Application.dataPath}";
#endif
            return key;
        }
    }
    
    /// This is an optional helper class to store your dbAddressOrName in PlayerPrefs
    /// Usage: If you have a new addressOrName, you want to clear your AuthToken
    ///   to prevent connection issues
    public static class DbAddressOrName
    {
        public static string AddressOrName => 
            PlayerPrefs.GetString(GetDbAddressOrNameKey());

        public static void SaveDbAddressOrName(string token) =>
            PlayerPrefs.SetString(GetDbAddressOrNameKey(), token);

        public static string GetDbAddressOrNameKey()
        {
            var key = "spacetimedb.db_address_or_name";
#if UNITY_EDITOR
            // Different editors need different keys
            key += $" - {Application.dataPath}";
#endif
            return key;
        }

        /// Checks new vs old DbAddressNameOrKey: If !match, reset AuthToken PlayerPrefs
        /// <returns>isMismatch</returns> 
        public static bool ResetAuthTokenOnMismatch(string newDbAddressOrName)
        {
            bool isMatch = AddressOrName == newDbAddressOrName;
            if (isMatch)
                return false; // !isMismatch: Auth token should match
            
            PlayerPrefs.DeleteKey(AuthToken.GetTokenKey());
            return true; // isMismatch: Deleted the old token PlayerPref
        }
    }

    public class NetworkManager : MonoBehaviour
    {
        protected void Awake()
        {
            // If you get a compile error on `Reducer`, that means you need to run the SpacetimeDB CLI generate command 
            SpacetimeDBClient.CreateInstance(new UnityDebugLogger());
        }

        private void OnDestroy()
        {
            SpacetimeDBClient.instance.Close();
        }

        private void Update()
        {
            SpacetimeDBClient.instance.Update();
        }        
    }
}
#endif