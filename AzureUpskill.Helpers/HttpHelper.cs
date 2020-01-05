using AzureUpskill.Core;
using AzureUpskill.Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
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
    }
}
