using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;
using Rtfx.Server.Models.Dtos;
using System.ComponentModel;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record ListFeedsRequest(
    [property: DefaultValue(0), BindFrom("skip")]
    int Skip,
    [property: DefaultValue(25), BindFrom("take")]
    int Take);

public sealed record ListFeedsResponse(FeedDto[] feeds);

public sealed class ListFeedsRequestValidator : Validator<ListFeedsRequest>
{
    public ListFeedsRequestValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.Take)
            .GreaterThan(0);
    }
}

public sealed class ListFeedsEndpoint : Endpoint<ListFeedsRequest, ListFeedsResponse>
{
    private readonly DatabaseContext _database;

    public ListFeedsEndpoint(DatabaseContext database)
    {
        _database = database;
    }

    public override void Configure()
    {
        Get("/api/feeds");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListFeedsRequest req, CancellationToken ct)
    {
        var feeds = await _database.Feeds
            .OrderBy(x => x.CreationDate)
            .Select(x => FeedDto.Create(HttpContext, x))
            .Skip(req.Skip)
            .Take(req.Take)
            .ToArrayAsync(ct);

        await SendOkAsync(new ListFeedsResponse(feeds), ct);
    }
}