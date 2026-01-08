using MongoDB.Bson.Serialization.Attributes;

namespace GuitarDb.Scraper.Models.Domain;

public class ConditionPricing
{
    [BsonElement("condition")]
    [BsonRequired]
    public GuitarCondition Condition { get; set; }

    [BsonElement("averagePrice")]
    public decimal? AveragePrice { get; set; }

    [BsonElement("minPrice")]
    public decimal? MinPrice { get; set; }

    [BsonElement("maxPrice")]
    public decimal? MaxPrice { get; set; }

    [BsonElement("listingCount")]
    public int ListingCount { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "USD";
}

public enum GuitarCondition
{
    BrandNew,
    Mint,
    Excellent,
    VeryGood,
    Good,
    Fair,
    Poor
}
