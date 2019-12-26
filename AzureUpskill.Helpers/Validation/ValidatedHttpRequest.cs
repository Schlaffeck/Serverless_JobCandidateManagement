using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;

namespace AzureUpskill.Helpers.Validation
{
    public class ValidatedHttpRequest<TBody>
    {
        private List<ValidationFailure> errorsList = new List<ValidationFailure>();

        public ValidatedHttpRequest(TBody body)
            : this(body, Enumerable.Empty<ValidationFailure>())
        {
        }

        public ValidatedHttpRequest(TBody body, IEnumerable<ValidationFailure> validationFailures)
        {
            Body = body;
            errorsList.AddRange(validationFailures);
        }

        public TBody Body { get; set; }

        public IEnumerable<ValidationFailure> Errors => errorsList;

        public bool IsValid => Errors.Count() == 0;

        public void AddErrors(IEnumerable<ValidationFailure> validationErrors)
        {
            errorsList.AddRange(validationErrors);
        }
    }
}
