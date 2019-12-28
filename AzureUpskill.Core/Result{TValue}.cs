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

        public Result(TValue value, IEnumerable<ValidationFailure> validationFailures)
        {
            Body = value;
            errorsList.AddRange(validationFailures);
        }

        public TValue Body { get; set; }

        public IEnumerable<ValidationFailure> Errors => errorsList;

        public bool IsValid => Errors.Count() == 0;

        public void AddErrors(IEnumerable<ValidationFailure> validationErrors)
        {
            errorsList.AddRange(validationErrors);
        }

        public string ToErrorString()
        {
            return string.Join("\r\n", this.Errors.Select(e => e.ToString()));
        }
    }
}
