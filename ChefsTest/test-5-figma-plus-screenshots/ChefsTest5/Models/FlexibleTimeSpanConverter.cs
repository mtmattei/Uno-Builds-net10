using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChefsTest5.Models;

public sealed class FlexibleTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            return string.IsNullOrEmpty(s) ? TimeSpan.Zero : TimeSpan.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            long ticks = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return TimeSpan.FromTicks(ticks);
                }
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var prop = reader.GetString();
                    reader.Read();
                    if (string.Equals(prop, "ticks", StringComparison.OrdinalIgnoreCase))
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
