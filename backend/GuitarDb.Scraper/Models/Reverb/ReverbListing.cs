using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuitarDb.Scraper.Models.Reverb;

public class ReverbListing
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("make")]
    public string Make { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public string? Year { get; set; }

    [JsonPropertyName("condition")]
    public ReverbCondition Condition { get; set; } = new();

    [JsonPropertyName("price")]
    public ReverbPrice Price { get; set; } = new();

    [JsonPropertyName("state")]
    public ReverbState State { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<ReverbCategory> Categories { get; set; } = new();

    [JsonIgnore]
    public int? ParsedYear => int.TryParse(Year, out var y) ? y : null;
}

public class ReverbCondition
{
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;
}

public class ReverbState
{
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class ReverbPrice
{
    [JsonPropertyName("amount")]
    [JsonConverter(typeof(StringToDecimalConverter))]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";
}

public class ReverbCategory
{
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;
}

public class StringToDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (decimal.TryParse(stringValue, out var result))
            {
                return result;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDecimal();
        }

        return 0;
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
