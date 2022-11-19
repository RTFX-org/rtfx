using FluentValidation;

namespace Rtfx.Server.Models;

public sealed record ArtifactMetadata(
    string FeedName,
    string PackageName,
    string SourceHash,
    string[] Tags,
    SourceVersionMetadata[] SourceVersions,
    Dictionary<string, string?> Metadata);

public sealed record SourceVersionMetadata(
    string Branch,
    string Commit);

public sealed class ArtifactMetadataValidator : AbstractValidator<ArtifactMetadata>
{
    public ArtifactMetadataValidator()
    {
        RuleFor(x => x.FeedName)
            .NotEmpty();
        RuleFor(x => x.PackageName)
            .NotEmpty();
        RuleFor(x => x.SourceHash)
            .NotEmpty()
            .Matches(RegularExpressions.SourceHash());
        RuleFor(x => x.Tags)
            .NotNull();
        RuleFor(x => x.SourceVersions)
            .NotEmpty()
            .ForEach(x => x.SetValidator(new SourceVersionMetadataValidator()));
        RuleFor(x => x.Metadata)
            .NotNull()
            .ForEach(d =>
                d.ChildRules(i =>
                    i.RuleFor(x => x.Key).NotEmpty()));
    }

    private sealed class SourceVersionMetadataValidator : AbstractValidator<SourceVersionMetadata>
    {
        public SourceVersionMetadataValidator()
        {
            RuleFor(x => x.Branch)
                .NotEmpty();
            RuleFor(x => x.Commit)
                .NotEmpty()
                .Matches(RegularExpressions.GitCommitHash());
        }
    }
}