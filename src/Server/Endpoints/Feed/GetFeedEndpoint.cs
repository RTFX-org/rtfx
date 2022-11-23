using FluentValidation;
using Rtfx.Server.Models;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record GetFeedRequest(long Id);

public sealed record GetFeedResponse(FeedDto Feed);

public sealed class GetFeedRequestValidator : Validator<GetFeedRequest>
{
    public GetFeedRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}

public sealed class GetFeedEndpoint : Endpoint<GetFeedRequest, GetFeedResponse>
{
    private readonly IFeedRepository _feedRepository;

    public GetFeedEndpoint(IFeedRepository feedRepository)
    {
        _feedRepository = feedRepository;
    }

    public override void Configure()
    {
        Get("/feeds/{Id}");
        Description(x => x
            .WithTags("Feeds")
            .ProducesProblemRtfx(Status400BadRequest)
            .ProducesProblemRtfx(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Gets a feed.";
            x.Responses[Status200OK] = "The feed was found.";
            x.Responses[Status404NotFound] = "The feed was not found.";
            x.ResponseExamples[Status404NotFound] = new RtfxErrorResponse
            {
                Errors.FeedWithIdDoesNotExist.GetError(1337),
            };
        });
    }

    public override async Task HandleAsync(GetFeedRequest req, CancellationToken ct)
    {
        var feed = await _feedRepository.TryGetFeedAsync(req.Id, ct);
        if (feed is null)
        {
            await this.SendErrorAsync(Status404NotFound, Errors.FeedWithIdDoesNotExist.GetError(req.Id), ct);
            return;
        }

        await SendOkAsync(new GetFeedResponse(FeedDto.Create(feed)), ct);
    }
}
