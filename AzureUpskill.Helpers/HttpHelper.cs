using System.Net;

namespace AzureUpskill.Helpers
{
    public static class HttpHelper
    {
        public static bool IsSuccess(this HttpStatusCode httpStatusCode)
        {
            var code = (int)httpStatusCode;
            return code >= 200 && code < 300;
        }
    }
}
