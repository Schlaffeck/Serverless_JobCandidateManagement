using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;

namespace AzureUpskill.Core
{
    public class Result<TValue>
    {
        private List<ValidationFailure> errorsList = new List<ValidationFailure>();

        public Result()
            : this(default(TValue))
        {
        }

        public Result(TValue value)
            : this(value, Enumerable.Empty<ValidationFailure>())
        {
        }

        public Result(IEnumerable<string> errors)
        {
            errorsList.AddRange(errors.Select(e => new ValidationFailure(string.Empty, e)));
        }

        public Result(params string[] errors)
        {
            errorsList.AddRange(errors.Select(e => new ValidationFailure(string.Empty, e)));
        }

        public Result(IEnumerable<ValidationFailure> validationFailures)
        {
            errorsList.AddRange(validationFailures);
        }

        public Result(TValue value, IEnumerable<ValidationFailure> validationFailures)
        {
            Value = value;
            errorsList.AddRange(validationFailures);
        }

        public TValue Value { get; set; }

        public IEnumerable<ValidationFailure> Errors => errorsList;

        public bool IsValid => Errors.Count() == 0;

        public void AddErrors(IEnumerable<ValidationFailure> validationErrors)
        {
            errorsList.AddRange(validationErrors);
        }

        public void AddError(ValidationFailure validationError)
        {
            errorsList.Add(validationError);
        }

        public void AddError(string propertyName, string errorMessage)
        {
            errorsList.Add(new ValidationFailure(propertyName, errorMessage));
        }

        public string ToErrorString()
        {
            return string.Join("\r\n", this.Errors.Select(e => e.ToString()));
        }
    }
}
