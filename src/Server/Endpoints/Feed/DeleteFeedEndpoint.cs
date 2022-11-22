using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;
using Rtfx.Server.Models;
using Rtfx.Server.Repositories;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record DeleteFeedRequest(long Id);

public sealed class DeleteFeedRequestValidator : Validator<DeleteFeedRequest>
{
    public DeleteFeedRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}

public sealed class DeleteFeedEndpoint : Endpoint<DeleteFeedRequest>
{
    private readonly IFeedRepository _feedRepository;

    public DeleteFeedEndpoint(IFeedRepository feedRepository)
    {
        _feedRepository = feedRepository;
    }

    public override void Configure()
    {
        Delete("/feeds/{Id}");
        Description(x => x
            .WithTags("Feeds")
            .DoesNotProduce(Status200OK)
            .Produces(Status202Accepted)
            .ProducesProblemRtfx(Status400BadRequest)
            .ProducesProblemRtfx(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Deletes a feed and all its packages and artifacts. Use with caution.";
            x.Responses[Status202Accepted] = "The feed has been deleted. Packages and Artifacts will be deleted asyncronously.";
            x.Responses[Status404NotFound] = "The feed was not found.";
            x.ResponseExamples[Status404NotFound] = new RtfxErrorResponse
            {
                Errors.FeedWithIdDoesNotExist.GetError(1337),
            };
        });
    }

    public override async Task HandleAsync(DeleteFeedRequest req, CancellationToken ct)
    {
        var feedExists = await _feedRepository.GetFeedExistAsync(req.Id, ct);
        if (!feedExists)
        {
            await this.SendErrorAsync(Status404NotFound, Errors.FeedWithIdDoesNotExist.GetError(req.Id), ct);
            return;
        }

        await _feedRepository.RemoveFeedAsync(req.Id, ct);

        await this.SendAcceptedAsync(ct);
    }
}
