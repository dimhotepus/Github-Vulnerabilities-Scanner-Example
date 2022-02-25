namespace Apptio.Dependencies.Scanner.Web.Constants
{
    public static class KnownErrorCodes
    {
        public static class Scan
        {
            private const string Prefix = "errors/scan/";

            public const string GenericApiFailure = Prefix + "generic_api_failure";

            public const string NotJson = Prefix + "not_json";
            public const string BadJsonBody = Prefix + "bad_json_body";
            public const string UnknownEcosystem = Prefix + "unknown_ecosystem";
            public const string MissedFileContent = Prefix + "missed_file_content";
            public const string FileContentNotBase64 = Prefix + "file_content_not_base64";
            public const string FileContentNotUtf8 = Prefix + "file_content_not_utf8";

            public const string FileContentNotPackageJson = Prefix + "file_content_not_package_json";

            public const string InternalScannerFailure = Prefix + "internal_scanner_failure";
        }
    }
}