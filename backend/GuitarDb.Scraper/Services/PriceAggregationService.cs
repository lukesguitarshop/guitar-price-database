using GuitarDb.Scraper.Models.Domain;
using GuitarDb.Scraper.Models.Reverb;
using Microsoft.Extensions.Logging;

namespace GuitarDb.Scraper.Services;

public class PriceAggregationService
{
    private readonly ILogger<PriceAggregationService> _logger;

    public PriceAggregationService(ILogger<PriceAggregationService> logger)
    {
        _logger = logger;
    }

    public Dictionary<string, List<ReverbListing>> GroupByGuitar(List<ReverbListing> listings)
    {
        var grouped = new Dictionary<string, List<ReverbListing>>();

        foreach (var listing in listings)
        {
            if (string.IsNullOrWhiteSpace(listing.Make) || string.IsNullOrWhiteSpace(listing.Model))
            {
                _logger.LogWarning("Skipping listing {Id} with missing make or model", listing.Id);
                continue;
            }

            var key = $"{listing.Make}|{listing.Model}|{listing.ParsedYear?.ToString() ?? "Unknown"}";

            if (!grouped.ContainsKey(key))
            {
                grouped[key] = new List<ReverbListing>();
            }

            grouped[key].Add(listing);
        }

        _logger.LogInformation("Grouped {Total} listings into {Groups} unique guitars",
            listings.Count, grouped.Count);

        return grouped;
    }

    public PriceSnapshot CalculatePriceSnapshot(List<ReverbListing> listings)
    {
        var snapshot = new PriceSnapshot
        {
            Date = DateTime.UtcNow.Date,
            ScrapedAt = DateTime.UtcNow,
            TotalListingsScraped = listings.Count,
            ConditionPricing = new List<ConditionPricing>()
        };

        var allConditions = Enum.GetValues<GuitarCondition>();

        foreach (var condition in allConditions)
        {
            var conditionListings = listings
                .Where(l => MapCondition(l.Condition.DisplayName) == condition)
                .ToList();

            var conditionPricing = new ConditionPricing
            {
                Condition = condition,
                ListingCount = conditionListings.Count
            };

            if (conditionListings.Any())
            {
                var prices = conditionListings.Select(l => l.Price.Amount).ToList();
                conditionPricing.AveragePrice = prices.Average();
                conditionPricing.MinPrice = prices.Min();
                conditionPricing.MaxPrice = prices.Max();
            }
            else
            {
                conditionPricing.AveragePrice = null;
                conditionPricing.MinPrice = null;
                conditionPricing.MaxPrice = null;
            }

            snapshot.ConditionPricing.Add(conditionPricing);
        }

        return snapshot;
    }

    private GuitarCondition MapCondition(string reverbCondition)
    {
        return reverbCondition.Trim().Replace(" ", "") switch
        {
            "BrandNew" => GuitarCondition.BrandNew,
            "Mint" => GuitarCondition.Mint,
            "Excellent" => GuitarCondition.Excellent,
            "VeryGood" => GuitarCondition.VeryGood,
            "Good" => GuitarCondition.Good,
            "Fair" => GuitarCondition.Fair,
            "Poor" => GuitarCondition.Poor,
            _ => throw new ArgumentException($"Unknown condition: {reverbCondition}")
        };
    }
}
