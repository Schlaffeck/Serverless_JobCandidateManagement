using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Core.Validation
{
    public interface IModelValidator
    {
        ValidationResult Validate(object model);
    }
}
