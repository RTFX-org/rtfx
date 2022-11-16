namespace Rtfx.Server.Extensions;

public static class EndpointExtensions
{
    public static async Task SendErrorAsync(this IEndpoint endpoint, int statusCode, string message, CancellationToken cancellation)
    {
        await endpoint.HttpContext.Response.SendErrorAsync(statusCode, message, cancellation);
    }
}
