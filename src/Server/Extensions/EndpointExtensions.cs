namespace Rtfx.Server.Extensions;

public static class EndpointExtensions
{
    public static async Task SendErrorAsync(this IEndpoint endpoint, int statusCode, string message, CancellationToken cancellation)
    {
        await endpoint.HttpContext.Response.SendErrorAsync(statusCode, message, cancellation);
    }

    public static async Task SendAsync(this IEndpoint endpoint, int statusCode, CancellationToken cancellation)
    {
        endpoint.HttpContext.Response.StatusCode = statusCode;
        await endpoint.HttpContext.Response.StartAsync(cancellation);
    }

    public static async Task SendAcceptedAsync(this IEndpoint endpoint, CancellationToken cancellation)
    {
        await endpoint.SendAsync(Status202Accepted, cancellation);
    }

    public static async Task SendCreatedAsync<T>(this IEndpoint endpoint, T response, CancellationToken cancellation)
    {
        await endpoint.HttpContext.Response.SendAsync(response, Status201Created, cancellation: cancellation);
    }
}
