using MongoDB.Bson.Serialization.Attributes;

namespace GuitarDb.API.Models;

public class PriceSnapshot
{
    [BsonElement("date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Date { get; set; }

    [BsonElement("source")]
    public string Source { get; set; } = string.Empty;

    [BsonElement("condition")]
    public string? Condition { get; set; }

    [BsonElement("avgPrice")]
    public decimal? AvgPrice { get; set; }

    [BsonElement("minPrice")]
    public decimal? MinPrice { get; set; }

    [BsonElement("maxPrice")]
    public decimal? MaxPrice { get; set; }

    [BsonElement("listingCount")]
    public int ListingCount { get; set; }

    [BsonElement("sampleListings")]
    public List<SimplifiedListing>? SampleListings { get; set; }
}

public class SimplifiedListing
{
    [BsonElement("listingId")]
    public string? ListingId { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("condition")]
    public string? Condition { get; set; }

    [BsonElement("url")]
    public string? Url { get; set; }

    [BsonElement("imageUrl")]
    public string? ImageUrl { get; set; }

    [BsonElement("listedDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ListedDate { get; set; }
}
