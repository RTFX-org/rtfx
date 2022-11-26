using FluentValidation;
using FluentValidation.Results;
using Rtfx.Server.Models;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record DeleteFeedRequest(string FeedId);

public sealed class DeleteFeedRequestValidator : Validator<DeleteFeedRequest>
{
    public DeleteFeedRequestValidator()
    {
        RuleFor(x => x.FeedId)
            .NotEmpty()
            .Matches(RegularExpressions.IdHash());
    }
}

public sealed class DeleteFeedEndpoint : Endpoint<DeleteFeedRequest>
{
    private readonly IFeedRepository _feedRepository;
    private readonly IIdHashingService _idHashingService;

    public DeleteFeedEndpoint(IFeedRepository feedRepository, IIdHashingService idHashingService)
    {
        _feedRepository = feedRepository;
        _idHashingService = idHashingService;
    }

    public override void Configure()
    {
        Delete("/feeds/{FeedId}");
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
            x.ResponseExamples[Status400BadRequest] = new RtfxErrorResponse
            {
                GetInvalidFeedIdHashError("[FeedId]"),
                RtfxError.DefaultExample,
            };
            x.ResponseExamples[Status404NotFound] = new RtfxErrorResponse
            {
                GetFeedWithIdDoesNotExistError("[FeedId]"),
            };
        });
    }

    public override async Task HandleAsync(DeleteFeedRequest req, CancellationToken ct)
    {
        if (!_idHashingService.TryDecodeId(req.FeedId, out long feedId))
        {
            await this.SendErrorAsync(Status400BadRequest, GetInvalidFeedIdHashError(req.FeedId), ct);
            return;
        }

        var feedExists = await _feedRepository.GetFeedExistAsync(feedId, ct);
        if (!feedExists)
        {
            await this.SendErrorAsync(Status404NotFound, GetFeedWithIdDoesNotExistError(req.FeedId), ct);
            return;
        }

        await _feedRepository.RemoveFeedAsync(feedId, ct);

        await this.SendAcceptedAsync(ct);
    }

    private static ValidationFailure GetInvalidFeedIdHashError(string feedId)
        => Errors.InvalidIdHash.GetError(feedId).WithPropertyName(nameof(DeleteFeedRequest.FeedId));

    private static ValidationFailure GetFeedWithIdDoesNotExistError(string feedId)
        => Errors.FeedWithIdDoesNotExist.GetError(feedId).WithPropertyName(nameof(DeleteFeedRequest.FeedId));
}
