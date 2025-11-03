using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyRoomSystem.Server.Converters;

public class FlexibleDateTimeOffsetConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();
        if (DateTime.TryParse(raw, out var dateTime))
            return dateTime.ToUniversalTime();
        
        throw new JsonException("无效 DateTime");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O"));
    }
}
