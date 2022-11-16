using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;
using Rtfx.Server.Models.Dtos;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record CreateFeedRequest(string Name);

public sealed record CreateFeedResponse(FeedDto Feed);

public sealed class CreateFeedRequestValidator : Validator<CreateFeedRequest>
{
    private static readonly Regex FeedNameRegex = new("""^[a-zA-Z\.\-_][a-zA-Z0-9\.\-_]*$""", RegexOptions.Compiled, TimeSpan.FromSeconds(2));

    public CreateFeedRequestValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(255)
            .Matches(FeedNameRegex)
            .WithMessage("The feed name cannot start with a digit and can only contain the following characters: a-z A-Z 0-9 . - _");
    }
}

public sealed class CreateFeedEndpoint : Endpoint<CreateFeedRequest, CreateFeedResponse>
{
    private readonly DatabaseContext _database;

    public CreateFeedEndpoint(DatabaseContext database)
    {
        _database = database;
    }

    public override void Configure()
    {
        Put("/api/feeds");
        AllowAnonymous();
        Description(x => x
            .WithName("CreateFeed")
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
        var feedExists = await _database.Feeds.AnyAsync(x => x.Name == req.Name);
        if (feedExists)
        {
            await this.SendErrorAsync(Status409Conflict, ErrorMessages.FeedWithNameAlreadyExists(req.Name), ct);
            return;
        }

        var feed = new Db.Feed
        {
            Name = req.Name,
        };

        _database.Feeds.Add(feed);
        await _database.SaveChangesAsync(ct);

        await SendAsync(new CreateFeedResponse(FeedDto.Create(HttpContext, feed)), Status201Created, ct);
    }
}