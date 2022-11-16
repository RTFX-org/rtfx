using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;
using Rtfx.Server.Models.Dtos;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record GetFeedRequest(string IdOrName);

public sealed record GetFeedResponse(FeedDto Feed);

public sealed class GetFeedRequestValidator : Validator<GetFeedRequest>
{
    public GetFeedRequestValidator()
    {
        RuleFor(x => x.IdOrName)
            .NotEmpty();
    }
}

public sealed class GetFeedEndpoint : Endpoint<GetFeedRequest, GetFeedResponse>
{
    private readonly DatabaseContext _database;

    public GetFeedEndpoint(DatabaseContext database)
    {
        _database = database;
    }

    public override void Configure()
    {
        Get("/api/feeds/{IdOrName}");
        AllowAnonymous();
        Description(x => x
            .WithName("GetFeed")
            .WithTags("Feeds")
            .ProducesProblemFE(Status400BadRequest));
        Summary(x =>
        {
            x.Summary = "Gets a feed.";
            x.Responses[Status200OK] = "The feed was found.";
            x.Responses[Status404NotFound] = "The feed was not found.";
        });
    }

    public override async Task HandleAsync(GetFeedRequest req, CancellationToken ct)
    {
        var feed = await GetFeedAsync(req.IdOrName, ct);
        if (feed is null)
            return;

        await SendAsync(new GetFeedResponse(FeedDto.Create(HttpContext, feed)), cancellation: ct);
    }

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:Field names should not use Hungarian notation", Justification = "False positive")]
    private async Task<Db.Feed?> GetFeedAsync(string idOrName, CancellationToken ct)
    {
        Db.Feed? feed = null;
        Func<string, string> errorMessageFunc;

        if (long.TryParse(idOrName, out var id))
        {
            feed = await _database.Feeds.FirstOrDefaultAsync(x => x.FeedId == id, ct);
            errorMessageFunc = static id => ErrorMessages.FeedWithIdDoesNotExist(id);
        }
        else
        {
            feed = await _database.Feeds.FirstOrDefaultAsync(x => x.Name == idOrName, ct);
            errorMessageFunc = static name => ErrorMessages.FeedWithNameDoesNotExist(name);
        }

        if (feed is null)
            await this.SendErrorAsync(Status404NotFound, errorMessageFunc(idOrName), ct);

        return feed;
    }
}
