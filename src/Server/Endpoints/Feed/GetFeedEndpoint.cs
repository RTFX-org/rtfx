using FluentValidation;
using FluentValidation.Results;
using Rtfx.Server.Models;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record GetFeedRequest(string FeedId);

public sealed record GetFeedResponse(FeedDto Feed);

public sealed class GetFeedRequestValidator : Validator<GetFeedRequest>
{
    public GetFeedRequestValidator()
    {
        RuleFor(x => x.FeedId)
            .NotEmpty()
            .Matches(RegularExpressions.IdHash());
    }
}

public sealed class GetFeedEndpoint : Endpoint<GetFeedRequest, GetFeedResponse>
{
    private readonly IFeedRepository _feedRepository;
    private readonly IIdHashingService _idHashingService;

    public GetFeedEndpoint(IFeedRepository feedRepository, IIdHashingService idHashingService)
    {
        _feedRepository = feedRepository;
        _idHashingService = idHashingService;
    }

    public override void Configure()
    {
        Get("/feeds/{FeedId}");
        Description(x => x
            .WithTags("Feeds")
            .ProducesProblemRtfx(Status400BadRequest)
            .ProducesProblemRtfx(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Gets a feed.";
            x.Responses[Status200OK] = "The feed was found.";
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

    public override async Task HandleAsync(GetFeedRequest req, CancellationToken ct)
    {
        if (!_idHashingService.TryDecodeId(req.FeedId, IdType.Feed, out long feedId))
        {
            await this.SendErrorAsync(Status400BadRequest, GetInvalidFeedIdHashError(req.FeedId), ct);
            return;
        }

        var feed = await _feedRepository.TryGetFeedAsync(feedId, ct);
        if (feed is null)
        {
            await this.SendErrorAsync(Status404NotFound, GetFeedWithIdDoesNotExistError(req.FeedId), ct);
            return;
        }

        await SendOkAsync(new GetFeedResponse(FeedDto.Create(feed, _idHashingService)), ct);
    }

    private static ValidationFailure GetInvalidFeedIdHashError(string feedId)
        => Errors.InvalidIdHash.GetError(feedId).WithPropertyName(nameof(GetFeedRequest.FeedId));

    private static ValidationFailure GetFeedWithIdDoesNotExistError(string feedId)
        => Errors.FeedWithIdDoesNotExist.GetError(feedId).WithPropertyName(nameof(GetFeedRequest.FeedId));
}
