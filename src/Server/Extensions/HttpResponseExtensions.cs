using FluentValidation.Results;

namespace Rtfx.Server.Extensions;

public static class HttpResponseExtensions
{
    public static async Task SendErrorAsync(this HttpResponse response, int statusCode, string message, CancellationToken cancellation)
    {
        await response.SendErrorsAsync(
            new List<ValidationFailure>(1)
            {
                new ValidationFailure("GeneralErrors", message),
            },
            statusCode,
            cancellation: cancellation);
    }
}
