using MaSch.Core.Extensions;
using Microsoft.AspNetCore.Http.Metadata;
using Rtfx.Server.Models;

namespace Rtfx.Server.Extensions;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder DoesNotProduce(this RouteHandlerBuilder builder, int statusCode)
    {
        builder.Add(b =>
        {
            var toRemove = b.Metadata
                .OfType<IProducesResponseTypeMetadata>()
                .Where(x => x.StatusCode == statusCode)
                .ToArray();
            ListExtensions.Remove(b.Metadata, toRemove);
        });
        return builder;
    }

    public static RouteHandlerBuilder ProducesProblemRtfx(this RouteHandlerBuilder builder, int statusCode)
    {
        return builder.ProducesProblemFE<RtfxErrorResponse>(statusCode);
    }
}
