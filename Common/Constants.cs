namespace Common
{
    public static class Constants
    {
        public const int SVC2_WS_PORT = 4300;
        public const int SVC2_NETTCP_PORT = 8086;
        public const int SVC3_WS_PORT = 4400;
        public const int SVC3_NETTCP_PORT = 8087;
        public const int SVC4_WS_PORT = 4500;
        public const int SVC4_NETTCP_PORT = 8088;

        public const string SVC_WS_HOST = "ws://localhost:";
        public const string SVC_WS_HOST_REMOTE = "ws://??:";

        public const string SB_CONFIG_SECTION = "SBConfig";
        public const string SB_CONN_STRING = "SasToken";
        public const string SB_TOPIC = "Topic";
        public const string SB_SUBSCRIPTION = "Subscription";

        public const string EH_CONFIG_SECTION = "EHConfig";
        public const string EH_HUB_PATH = "HubPath";
        public const string EH_SENDTO_HUB_PATH = "SendToPath";
        public const string EH_CONSUMER_GROUP = "ConsumerGroup";
        public const string EH_CONN_STRING = "ConnString";
        public const string EH_STORAGE_CONN_STRING = "StorageAccount";

        public const string DOTNETTY_SIMPLE_ENDPOINT = "DotNettySimpleTcp";

    }
}
