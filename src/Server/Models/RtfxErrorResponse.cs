using FluentValidation.Results;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rtfx.Server.Models;

[JsonConverter(typeof(RtfxErrorResponseConverter))]
[Newtonsoft.Json.JsonObject]
public sealed class RtfxErrorResponse : IEnumerable
{
    public static RtfxErrorResponse DefaultExample => new RtfxErrorResponse { RtfxError.DefaultExample };

    public List<RtfxError> Errors { get; set; } = new();

    public RtfxErrorResponse Add(ValidationFailure failure)
    {
        Errors.Add(new RtfxError(failure));
        return this;
    }

    public RtfxErrorResponse Add(IEnumerable<ValidationFailure> failures)
    {
        Errors.AddRange(failures.Select(x => new RtfxError(x)));
        return this;
    }

    public RtfxErrorResponse Add(RtfxError error)
    {
        Errors.Add(error);
        return this;
    }

    public RtfxErrorResponse Add(IEnumerable<RtfxError> errors)
    {
        Errors.AddRange(errors);
        return this;
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

    public static RtfxError DefaultExample => new RtfxError { PropertyName = "[...]", ErrorCode = "string", Message = "string", AttemptedValue = "any" };

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