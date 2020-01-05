using AzureUpskill.Core.Validation;
using AzureUpskill.Functions.Validation;
using AzureUpskill.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.Filters
{
    [Obsolete]
    public class HttpRequestValidatorAttribute : FunctionInvocationFilterAttribute
    {
        public Type DataType { get; private set; }

        public Type ValidatorType { get; private set; }

        public HttpRequestValidatorAttribute(Type dataType, Type validatorType)
        {
            DataType = dataType;
            ValidatorType = validatorType;
        }

        public override async Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            var value = executingContext.Arguments.Values.FirstOrDefault(v => v.GetType() == DataType);
            if (value is null)
            {
                var requestParam = executingContext.Arguments.Values.FirstOrDefault(v => v is HttpRequest) as HttpRequest;
                if (requestParam != null)
                {
                    value = requestParam.GetJsonBodyAsync(DataType);
                }
            }
            var result = value.Validate(ValidatorType);
            if (!result.IsValid)
            {
                throw new ModelValidationException(result.ToErrorString());
            }

            await base.OnExecutingAsync(executingContext, cancellationToken);
        }
    }
}
