using AzureUpskill.Core;
using AzureUpskill.Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AzureUpskill.Helpers
{
    public static class HttpHelper
    {
        public static bool IsSuccess(this HttpStatusCode httpStatusCode)
        {
            var code = (int)httpStatusCode;
            return code >= 200 && code < 300;
        }

        public static bool IsSuccessStatusCode(this int httpStatusCodeInt)
        {
            var code = httpStatusCodeInt;
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
        public static async Task<Result<T>> GetJsonBodyValidatedAsync<T, V>(this HttpRequest request)
            where V : AbstractValidator<T>, new()
        {
            var requestObject = await request.GetJsonBodyAsync<T>();
            var validator = new V();
            var validationResult = validator.Validate(requestObject);

            if (!validationResult.IsValid)
            {
                return new Result<T>(requestObject, validationResult.Errors);
            }

            return new Result<T>(requestObject);
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
        public static async Task<Result<object>> GetJsonBodyValidatedAsync(this HttpRequest request, Type dataType, Type validatorType)
        {
            var requestObject = await request.GetJsonBodyAsync(dataType);
            var validator = Activator.CreateInstance(validatorType) as IModelValidator;
            if (!(validator is null))
            {
                var validationResult = validator.Validate(requestObject);

                if (!validationResult.IsValid)
                {
                    return new Result<object>(requestObject, validationResult.Errors);
                }
            }

            return new Result<object>(requestObject);
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

        /// <summary>
        /// Returns the deserialized request body.
        /// </summary>
        /// <param name="dataType">Type used for deserialization of the request body.</typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<object> GetJsonBodyAsync(this HttpRequest request, Type dataType)
        {
            using (var streamReader = new StreamReader(request.Body))
            {
                string requestBody = await streamReader.ReadToEndAsync();

                return JsonConvert.DeserializeObject(requestBody, dataType);
            }
        }

        public static string ToErrorString(this HttpResponse httpResponse)
        {
            using (var streamReader = new StreamReader(httpResponse.Body))
            {
                return $"{httpResponse.StatusCode} - {streamReader.ReadToEnd()}"; 
            }
        }

        public static string ReadStringToEnd(this Stream responseStream)
        {
            using (var streamReader = new StreamReader(responseStream))
            {
                return $"{streamReader.ReadToEnd()}";
            }
        }

        public static T GetObjectFromQueryString<T>(this HttpRequest request)
            where T : class, new()
        {
            var dict = HttpUtility.ParseQueryString(request.QueryString.ToString());
            string json = JsonConvert.SerializeObject(dict.Cast<string>().ToDictionary(k => k, v => dict[v]));
            var respObj = JsonConvert.DeserializeObject<T>(json);
            return respObj;
        }

        public static string ToQueryStringNoValues(this object obj)
        {
            var qs = new StringBuilder("?");

            var objType = obj.GetType();

            foreach(var p in objType.GetProperties())
            {
                    qs.Append($"{Uri.EscapeDataString(p.Name)}={{{p.Name}}}&");
            }

            return qs.ToString();
        }

    }
}
