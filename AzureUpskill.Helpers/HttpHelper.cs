using AzureUpskill.Helpers.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AzureUpskill.Helpers
{
    public static class HttpHelper
    {
        public static bool IsSuccess(this HttpStatusCode httpStatusCode)
        {
            var code = (int)httpStatusCode;
            return code >= 200 && code < 300;
        }

        /// <summary>
        /// Returns the deserialized request body with validation information.
        /// </summary>
        /// <typeparam name="T">Type used for deserialization of the request body.</typeparam>
        /// <typeparam name="V">
        /// Validator used to validate the deserialized request body.
        /// </typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<ValidatedHttpRequest<T>> GetJsonBodyValidatedAsync<T, V>(this HttpRequest request)
            where V : AbstractValidator<T>, new()
        {
            var requestObject = await request.GetJsonBodyAsync<T>();
            var validator = new V();
            var validationResult = validator.Validate(requestObject);

            if (!validationResult.IsValid)
            {
                return new ValidatedHttpRequest<T>(requestObject, validationResult.Errors);
            }

            return new ValidatedHttpRequest<T>(requestObject);
        }

        /// <summary>
        /// Returns the deserialized request body.
        /// </summary>
        /// <typeparam name="T">Type used for deserialization of the request body.</typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<T> GetJsonBodyAsync<T>(this HttpRequest request)
        {
            using (var streamReader = new StreamReader(request.Body))
            {
                string requestBody = await streamReader.ReadToEndAsync();

                return JsonConvert.DeserializeObject<T>(requestBody);
            }
        }
    }
}
