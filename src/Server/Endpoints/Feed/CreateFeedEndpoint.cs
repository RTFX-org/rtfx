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
        Description(x => x.WithName("CreateFeed").WithGroupName("Feeds"));
        Summary(x =>
        {
        });
    }

    public override async Task HandleAsync(CreateFeedRequest req, CancellationToken ct)
    {
        var feedExists = await _database.Feeds.AnyAsync(x => x.Name == req.Name, ct);
        if (feedExists)
        {
            await SendStringAsync($"A feed with the name \"{req.Name}\" already exists.", statusCode: StatusCodes.Status409Conflict, cancellation: ct);
        }
        else
        {
            var feed = new Database.Entities.Feed { Name = req.Name };
            await _database.Feeds.AddAsync(feed, ct);
            await _database.SaveChangesAsync(ct);

            await SendAsync(new CreateFeedResponse(FeedDto.Create(HttpContext, feed)), StatusCodes.Status201Created, ct);
        }
    }
}