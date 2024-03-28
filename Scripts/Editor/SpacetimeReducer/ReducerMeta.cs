using static SpacetimeMeta;

namespace SpacetimeDB.Editor
{
    /// Static metadata for PublisherWindow
    public static class ReducerMeta
    {
        public const string REDUCER_DIR_PATH = "Packages/" + SDK_PACKAGE_NAME + "/Scripts/Editor/SpacetimeReducer";
        public static string PathToUxml => $"{REDUCER_DIR_PATH}/ReducerWindowComponents.uxml";
        public static string PathToUss => $"{REDUCER_DIR_PATH}/ReducerWindowStyles.uss";
        public const string TOP_BANNER_CLICK_LINK = "https://spacetimedb.com/docs/modules";
    }
}