using FluentValidation.Results;

namespace Rtfx.Server.Extensions;

public static class HttpResponseExtensions
{
    public static async Task SendErrorAsync(this HttpResponse response, int statusCode, ValidationFailure error, CancellationToken cancellation)
    {
        await response.SendErrorsAsync(
            new List<ValidationFailure>(1) { error },
            statusCode,
            cancellation: cancellation);
    }
}
