using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GuitarDb.API.Models;

public class Guitar
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("brand")]
    [BsonRequired]
    public string Brand { get; set; } = string.Empty;

    [BsonElement("model")]
    [BsonRequired]
    public string Model { get; set; } = string.Empty;

    [BsonElement("year")]
    public int? Year { get; set; }

    [BsonElement("finish")]
    public string? Finish { get; set; }

    [BsonElement("category")]
    public string? Category { get; set; }

    [BsonElement("specs")]
    public GuitarSpecs? Specs { get; set; }

    [BsonElement("images")]
    public List<string>? Images { get; set; }

    [BsonElement("priceHistory")]
    public List<PriceSnapshot>? PriceHistory { get; set; }

    [BsonElement("metadata")]
    public GuitarMetadata Metadata { get; set; } = new();
}

public class GuitarMetadata
{
    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastUpdated")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
