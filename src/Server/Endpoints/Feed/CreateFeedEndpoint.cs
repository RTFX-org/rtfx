using FluentValidation;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record CreateFeedRequest(string Name);

public sealed record CreateFeedResponse(FeedDto Feed);

public sealed class CreateFeedRequestValidator : Validator<CreateFeedRequest>
{
    public CreateFeedRequestValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(255)
            .Matches(RegularExpressions.FeedName())
            .WithMessage("The feed name cannot start with a digit and can only contain the following characters: a-z A-Z 0-9 . - _");
    }
}

public sealed class CreateFeedEndpoint : Endpoint<CreateFeedRequest, CreateFeedResponse>
{
    private readonly IFeedRepository _feedRepository;

    public CreateFeedEndpoint(IFeedRepository feedRepository)
    {
        _feedRepository = feedRepository;
    }

    public override void Configure()
    {
        Put("/feeds");
        Description(x => x
            .WithTags("Feeds")
            .DoesNotProduce(Status200OK)
            .Produces<CreateFeedResponse>(Status201Created)
            .ProducesProblemFE(Status400BadRequest)
            .ProducesProblemFE(Status409Conflict));
        Summary(x =>
        {
            x.Summary = "Creates a new feed.";
            x.Responses[Status201Created] = "The feed has been created successfully.";
            x.Responses[Status409Conflict] = ErrorMessages.FeedWithNameAlreadyExists("[name]");
        });
    }

    public override async Task HandleAsync(CreateFeedRequest req, CancellationToken ct)
    {
        var feedExists = await _feedRepository.GetFeedExistAsync(req.Name, ct);
        if (feedExists)
        {
            await this.SendErrorAsync(Status409Conflict, ErrorMessages.FeedWithNameAlreadyExists(req.Name), ct);
            return;
        }

        var feed = new Db.Feed
        {
            Name = req.Name,
        };
        await _feedRepository.InsertFeedAsync(feed, ct);

        await SendAsync(new CreateFeedResponse(FeedDto.Create(HttpContext, feed)), Status201Created, ct);
    }
}