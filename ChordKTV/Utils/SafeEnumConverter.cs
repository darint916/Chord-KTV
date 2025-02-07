using System.Text.Json;
using System.Text.Json.Serialization;
namespace ChordKTV.Utils;


public class SafeEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (Enum.TryParse(reader.GetString(), true, out T result))
            {
                return result;
            }
        }
        return default; // Defaults to first enum value (UNK in your case)
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
