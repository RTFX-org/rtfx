using FluentValidation.Results;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rtfx.Server.Models;

[JsonConverter(typeof(RtfxErrorResponseConverter))]
[Newtonsoft.Json.JsonObject]
public sealed class RtfxErrorResponse : IEnumerable
{
    public List<RtfxError> Errors { get; set; } = new();

    public void Add(ValidationFailure failure)
    {
        Errors.Add(new RtfxError(failure));
    }

    public void Add(IEnumerable<ValidationFailure> failures)
    {
        Errors.AddRange(failures.Select(x => new RtfxError(x)));
    }

    public void Add(RtfxError error)
    {
        Errors.Add(error);
    }

    public void Add(IEnumerable<RtfxError> errors)
    {
        Errors.AddRange(errors);
    }

    public IEnumerator GetEnumerator() => Errors.GetEnumerator();
}

public sealed class RtfxError
{
    public RtfxError()
    {
    }

    [SetsRequiredMembers]
    public RtfxError(ValidationFailure failure)
    {
        PropertyName = failure.PropertyName;
        ErrorCode = failure.ErrorCode;
        Message = failure.ErrorMessage;
        AttemptedValue = failure.AttemptedValue;
    }

    public string? PropertyName { get; init; }
    public required string ErrorCode { get; init; }
    public required string Message { get; init; }
    public required object AttemptedValue { get; init; }
}

public sealed class RtfxErrorResponseConverter : JsonConverter<RtfxErrorResponse>
{
    public override RtfxErrorResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, RtfxErrorResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("errors");
        JsonSerializer.Serialize(writer, value.Errors, options);
        writer.WriteEndObject();
    }
}