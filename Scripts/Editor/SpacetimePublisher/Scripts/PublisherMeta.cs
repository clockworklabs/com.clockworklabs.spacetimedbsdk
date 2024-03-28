namespace SpacetimeDB.Editor
{
    using static SpacetimeMeta;
    
    /// Static metadata for PublisherWindow
    public static class PublisherMeta
    {
        public enum FoldoutGroupType
        {
            Server,
            Identity,
            Publish,
            PublishResult,
        }

        public const string TOP_BANNER_CLICK_LINK = "https://spacetimedb.com/docs/modules";
        public const string DOCS_URL = "https://spacetimedb.com/install";
        public const string PUBLISHER_DIR_PATH = "Packages/" + SDK_PACKAGE_NAME + "/Scripts/Editor/SpacetimePublisher";
        public static string PathToUxml => $"{PUBLISHER_DIR_PATH}/PublisherWindowComponents.uxml";
        public static string PathToUss => $"{PUBLISHER_DIR_PATH}/PublisherWindowStyles.uss";
        public static string PathToAutogenDir => $"{UnityEngine.Application.dataPath}/StdbAutogen";
    }
}