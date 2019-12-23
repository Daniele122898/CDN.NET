namespace CDN.NET.Wrapper.Utils
{
    public static class Endpoints
    {
        // Authentication
        public static string Login = "api/auth/login";
        public static string Register = "api/auth/register";
        public static string ApiKey = "api/auth/apikey";
        public static string AuthTest = "api/auth/test";
        
        // File Upload
        public static string FileUpload = "api/upload";
        public static string FileUploadMulti = "api/upload/multi";
        
        // File Endpoints
        public static string FileRemove = "api/file";
        public static string FileRemoveMulti = "api/file/multi";
    }
}