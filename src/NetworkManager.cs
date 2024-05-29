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
}
#endif
