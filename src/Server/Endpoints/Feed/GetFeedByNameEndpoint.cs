using FluentValidation;
using Rtfx.Server.Models;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record GetFeedByNameRequest(string Name);

public sealed record GetFeedByNameResponse(FeedDto Feed);

public sealed class GetFeedByNameRequestValidator : Validator<GetFeedByNameRequest>
{
    public GetFeedByNameRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
    }
}

public class GetFeedByNameEndpoint : Endpoint<GetFeedByNameRequest, GetFeedByNameResponse>
{
    private readonly IFeedRepository _feedRepository;

    public GetFeedByNameEndpoint(IFeedRepository feedRepository)
    {
        _feedRepository = feedRepository;
    }

    public override void Configure()
    {
        Get("/feeds/feed");
        Description(x => x
            .WithTags("Feeds")
            .ProducesProblemRtfx(Status400BadRequest)
            .ProducesProblemRtfx(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Gets a feed by its name.";
            x.Responses[Status200OK] = "The feed was found.";
            x.Responses[Status404NotFound] = "The feed was not found.";
            x.ResponseExamples[Status404NotFound] = new RtfxErrorResponse
            {
                Errors.FeedWithNameDoesNotExist.GetError("[FeedName]"),
            };
        });
    }

    public override async Task HandleAsync(GetFeedByNameRequest req, CancellationToken ct)
    {
        var feed = await _feedRepository.TryGetFeedAsync(req.Name, ct);
        if (feed is null)
        {
            await this.SendErrorAsync(Status404NotFound, Errors.FeedWithNameDoesNotExist.GetError(req.Name), ct);
            return;
        }

        await SendOkAsync(new GetFeedByNameResponse(FeedDto.Create(feed)), ct);
    }
}
