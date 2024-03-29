﻿using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;
using System.ComponentModel;

namespace Rtfx.Server.Endpoints.Feed;

public sealed partial record ListFeedsRequest(
    [property: DefaultValue(0)]
    int? Skip,
    [property: DefaultValue(25)]
    int? Take);

public sealed record ListFeedsResponse(FeedDto[] Feeds);

public sealed class ListFeedsRequestValidator : Validator<ListFeedsRequest>
{
    public ListFeedsRequestValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Skip.HasValue);
        RuleFor(x => x.Take)
            .GreaterThan(0)
            .When(x => x.Take.HasValue);
    }
}

public sealed class ListFeedsEndpoint : Endpoint<ListFeedsRequest, ListFeedsResponse>
{
    private readonly IFeedRepository _feedRepository;
    private readonly IIdHashingService _idHashingService;

    public ListFeedsEndpoint(IFeedRepository feedRepository, IIdHashingService idHashingService)
    {
        _feedRepository = feedRepository;
        _idHashingService = idHashingService;
    }

    public override void Configure()
    {
        Get("/feeds");
        Description(x => x
            .WithTags("Feeds")
            .ProducesProblemRtfx(Status400BadRequest));
        Summary(x =>
        {
            x.Summary = "Lists all available feeds.";
            x.Responses[Status200OK] = "The list of feeds has ben successfully retrieved.";
        });
    }

    public override async Task HandleAsync(ListFeedsRequest req, CancellationToken ct)
    {
        var feeds = await _feedRepository
            .GetFeeds(req.Skip ?? 0, req.Take ?? 25)
            .Select(x => FeedDto.Create(x, _idHashingService))
            .ToArrayAsync();

        await SendOkAsync(new ListFeedsResponse(feeds), ct);
    }
}