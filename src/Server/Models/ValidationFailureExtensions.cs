using FluentValidation.Results;

namespace Rtfx.Server.Models;

public static class ValidationFailureExtensions
{
    public static ValidationFailure WithPropertyName(this ValidationFailure validationFailure, string propertyName)
    {
        validationFailure.PropertyName = propertyName;
        return validationFailure;
    }
}
