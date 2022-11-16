using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record DeleteFeedRequest(string IdOrName);

public sealed class DeleteFeedRequestValidator : Validator<DeleteFeedRequest>
{
    public DeleteFeedRequestValidator()
    {
        RuleFor(x => x.IdOrName)
            .NotEmpty();
    }
}

public sealed class DeleteFeedEndpoint : Endpoint<DeleteFeedRequest>
{
    private readonly DatabaseContext _database;

    public DeleteFeedEndpoint(DatabaseContext databaseContext)
    {
        _database = databaseContext;
    }

    public override void Configure()
    {
        Delete("/api/feeds/{IdOrName}");
        AllowAnonymous();
        Description(x => x
            .WithName("DeleteFeed")
            .WithTags("Feeds")
            .DoesNotProduce(Status200OK)
            .Produces(Status202Accepted)
            .ProducesProblemFE(Status400BadRequest)
            .ProducesProblemFE(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Deletes a feed and all its packages and artifacts. Use with caution.";
            x.Responses[Status202Accepted] = "The feed has been deleted. Packages and Artifacts will be deleted asyncronously.";
            x.Responses[Status404NotFound] = "The feed was not found.";
        });
    }

    public override async Task HandleAsync(DeleteFeedRequest req, CancellationToken ct)
    {
        var feedId = await GetFeedIdAsync(req.IdOrName, ct);
        if (feedId <= 0)
            return;

        var feed = new Db.Feed
        {
            FeedId = feedId,
            Name = null!,
        };

        _database.Remove(feed);
        await _database.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = Status202Accepted;
        await HttpContext.Response.StartAsync(ct);
    }

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:Field names should not use Hungarian notation", Justification = "False positive")]
    private async Task<long> GetFeedIdAsync(string idOrName, CancellationToken ct)
    {
        long feedId;
        Func<string, string> errorMessageFunc;

        if (long.TryParse(idOrName, out var id))
        {
            feedId = await _database.Feeds.AnyAsync(x => x.FeedId == id, ct) ? id : 0;
            errorMessageFunc = static id => ErrorMessages.FeedWithIdDoesNotExist(id);
        }
        else
        {
            feedId = await _database.Feeds.Where(x => x.Name == idOrName).Select(x => x.FeedId).FirstOrDefaultAsync(ct);
            errorMessageFunc = static name => ErrorMessages.FeedWithNameDoesNotExist(name);
        }

        if (feedId <= 0)
            await this.SendErrorAsync(Status404NotFound, errorMessageFunc(idOrName), ct);

        return feedId;
    }
}
