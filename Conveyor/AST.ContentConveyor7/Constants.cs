namespace AST.ContentConveyor7
{
    public static class Constants
    {
        public const string ApplicationName = "Content Conveyor";

        public const string ApplicationAlias = "ContentConveyor";

        public const string TreeName = "Conveyor";

        public const string TreeAlias = "ASTContentConveyor";

        public const string ContentFileName = "ContentConveyor.xml";

        public const string UploadDataTypeGuid = "5032a6e6-69e3-491d-bb28-cd31cd11086c";

        public static readonly string[] MediaDefaultProperties =
        {
            "umbracoWidth", "umbracoHeight", "umbracoBytes",
            "umbracoExtension"
        };

        public static class Examine
        {
            public const string InternalIndexer = "InternalIndexer";
            public const string ExternalIndexer = "ExternalIndexer";
            public const string InternalMemberIndexer = "InternalMemberIndexer";

            public const string InternalSearcher = "InternalSearcher";
            public const string ExternalSearcher = "ExternalSearcher";
            public const string InternalMemberSearcher = "InternalMemberSearcher";
        }
    }
}