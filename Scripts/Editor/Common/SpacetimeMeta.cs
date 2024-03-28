/// Static metadata for SpacetimeDB editor scripts
public static class SpacetimeMeta
{
    #region Names & Paths
    public const string SDK_PACKAGE_NAME = "com.clockworklabs.spacetimedbsdk";
    public const string COMMON_DIR_PATH = "Packages/" + SDK_PACKAGE_NAME + "/Scripts/Editor/Common";
    
    /// Path to common SpacetimeDB Editor USS styles
    public static string PathToCommonUss => $"{COMMON_DIR_PATH}/CommonStyles.uss";
    #endregion // Names & Paths
    

    #region Colors & Formatting
    // Colors pulled from docs, often coincided with UI elements >>
    public const string ACTION_COLOR_HEX = "#FFEA30"; // Corn Yellow
    public const string ERROR_COLOR_HEX = "#FDBE01"; // Golden Orange
    public const string SUCCESS_COLOR_HEX = "#4CF490"; // Sea Green
    public const string INPUT_TEXT_COLOR = "#B6C0CF"; // Hazel Grey
    
    public enum StringStyle
    {
        Action,
        Error,
        Success,
    }
        
    public static string GetStyledStr(StringStyle style, string str)
    {
        return style switch
        {
            StringStyle.Action => $"<color={ACTION_COLOR_HEX}>{str}</color>",
            StringStyle.Error => $"<color={ERROR_COLOR_HEX}>{str}</color>",
            StringStyle.Success => $"<color={SUCCESS_COLOR_HEX}>{str}</color>",
        };
    }
    #endregion // Colors & Formatting
    

    #region Editor Pref Keys
    public const string EDITOR_PREFS_MODULE_NAME_KEY = "SPACETIMEDB_MODULE_NAME"; 
    #endregion // Editor Pref Keys
}
