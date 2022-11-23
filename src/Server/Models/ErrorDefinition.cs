using FluentValidation.Results;

namespace Rtfx.Server.Models;

public class ErrorDefinition
{
    public ErrorDefinition(string code)
    {
        Code = code;
    }

    public string Code { get; }

    protected ValidationFailure CreateFailure(string message, object? attemptedValue)
    {
        return new ValidationFailure
        {
            ErrorCode = Code,
            ErrorMessage = message,
            AttemptedValue = attemptedValue,
        };
    }
}