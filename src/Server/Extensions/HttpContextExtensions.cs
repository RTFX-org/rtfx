namespace Rtfx.Server.Extensions;

public static class HttpContextExtensions
{
    public static Uri GetBaseUri(this HttpContext context)
    {
        var builder = new UriBuilder
        {
            Scheme = context.Request.Scheme,
            Host = context.Request.Host.Host,
            Path = context.Request.PathBase,
        };

        var port = context.Request.Host.Port;
        if (port.HasValue)
            builder.Port = port.Value;

        return builder.Uri;
    }
}