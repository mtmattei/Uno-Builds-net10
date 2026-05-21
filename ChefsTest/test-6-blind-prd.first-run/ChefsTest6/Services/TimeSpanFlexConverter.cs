using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChefsTest6.Services;

public sealed class TimeSpanFlexConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (TimeSpan.TryParse(s, out var ts))
            {
                return ts;
            }
            return TimeSpan.Zero;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            long ticks = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var prop = reader.GetString();
                    reader.Read();
                    if (string.Equals(prop, "ticks", StringComparison.OrdinalIgnoreCase) &&
                        reader.TokenType == JsonTokenType.Number)
                    {
                        ticks = reader.GetInt64();
                    }
                }
            }
            return TimeSpan.FromTicks(ticks);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return TimeSpan.FromTicks(reader.GetInt64());
        }

        return TimeSpan.Zero;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("c"));
    }
}
