using MongoDB.Bson.Serialization.Attributes;

namespace GuitarDb.Scraper.Models.Domain;

public class PriceSnapshot
{
    [BsonElement("date")]
    [BsonRequired]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Date { get; set; }

    [BsonElement("conditionPricing")]
    public List<ConditionPricing> ConditionPricing { get; set; } = new();

    [BsonElement("totalListingsScraped")]
    public int TotalListingsScraped { get; set; }

    [BsonElement("scrapedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;
}
