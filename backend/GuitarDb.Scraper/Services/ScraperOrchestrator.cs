using GuitarDb.Scraper.Models.Domain;
using Microsoft.Extensions.Logging;

namespace GuitarDb.Scraper.Services;

public class ScraperOrchestrator
{
    private readonly ReverbApiClient _apiClient;
    private readonly GuitarRepository _repository;
    private readonly PriceAggregationService _aggregationService;
    private readonly ILogger<ScraperOrchestrator> _logger;

    public ScraperOrchestrator(
        ReverbApiClient apiClient,
        GuitarRepository repository,
        PriceAggregationService aggregationService,
        ILogger<ScraperOrchestrator> logger)
    {
        _apiClient = apiClient;
        _repository = repository;
        _aggregationService = aggregationService;
        _logger = logger;
    }

    public async Task RunAsync(string query, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("===== Starting Guitar Price Scraper =====");
        _logger.LogInformation("Query: {Query}", query);
        _logger.LogInformation("Start Time: {Time:yyyy-MM-dd HH:mm:ss} UTC", startTime);

        var stats = new ScraperStats { StartTime = startTime };

        try
        {
            _logger.LogInformation("Step 1: Fetching listings from Reverb API...");
            var searchResult = await _apiClient.SearchGuitarsAsync(query, cancellationToken);
            stats.TotalListingsFetched = searchResult.TotalListingsFetched;
            stats.ApiCallsMade = searchResult.ApiCallsMade;

            if (searchResult.Listings.Count == 0)
            {
                _logger.LogWarning("No listings found for query: {Query}", query);
                PrintSummary(stats);
                return;
            }

            _logger.LogInformation("Step 2: Grouping listings by guitar (Make + Model + Year)...");
            var guitarGroups = _aggregationService.GroupByGuitar(searchResult.Listings);
            stats.TotalGuitars = guitarGroups.Count;

            _logger.LogInformation("Step 3: Processing {Count} unique guitars...", guitarGroups.Count);

            var processed = 0;
            foreach (var (key, guitarListings) in guitarGroups)
            {
                processed++;

                try
                {
                    var parts = key.Split('|');
                    var make = parts[0];
                    var model = parts[1];
                    var yearStr = parts[2];
                    var year = yearStr == "Unknown" ? (int?)null : int.Parse(yearStr);

                    _logger.LogInformation("[{Current}/{Total}] Processing: {Make} {Model} ({Year})",
                        processed, guitarGroups.Count, make, model, year?.ToString() ?? "Unknown Year");

                    var snapshot = _aggregationService.CalculatePriceSnapshot(guitarListings);
                    _logger.LogDebug("  - Calculated prices for {Count} listings", guitarListings.Count);

                    var guitar = await _repository.FindByUniqueKeyAsync(make, model, year, cancellationToken);

                    if (guitar == null)
                    {
                        guitar = new Guitar
                        {
                            Make = make,
                            Model = model,
                            Year = year,
                            PriceHistory = new List<PriceSnapshot> { snapshot }
                        };

                        guitar = await _repository.UpsertGuitarAsync(guitar, cancellationToken);
                        stats.GuitarsCreated++;
                        _logger.LogInformation("  - Created new guitar document");
                    }
                    else
                    {
                        await _repository.AppendPriceSnapshotAsync(guitar.Id, snapshot, cancellationToken);
                        stats.GuitarsUpdated++;
                        _logger.LogInformation("  - Appended snapshot to existing guitar");
                    }

                    stats.GuitarsProcessed++;
                }
                catch (Exception ex)
                {
                    stats.Errors++;
                    _logger.LogError(ex, "  - Error processing guitar: {Key}", key);
                }
            }

            stats.EndTime = DateTime.UtcNow;
            PrintSummary(stats);
        }
        catch (Exception ex)
        {
            stats.EndTime = DateTime.UtcNow;
            _logger.LogError(ex, "Fatal error during scraping");
            PrintSummary(stats);
            throw;
        }
    }

    private void PrintSummary(ScraperStats stats)
    {
        var duration = stats.EndTime - stats.StartTime;

        _logger.LogInformation("");
        _logger.LogInformation("╔═══════════════════════════════════════════════════════════════╗");
        _logger.LogInformation("║              GUITAR PRICE SCRAPER SUMMARY                     ║");
        _logger.LogInformation("╠═══════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║ API Statistics:                                               ║");
        _logger.LogInformation("║   • API Calls Made: {ApiCalls,-45} ║", stats.ApiCallsMade);
        _logger.LogInformation("║   • Total Listings Fetched: {Listings,-36} ║", stats.TotalListingsFetched);
        _logger.LogInformation("║                                                               ║");
        _logger.LogInformation("║ Guitar Statistics:                                            ║");
        _logger.LogInformation("║   • Unique Guitars Found: {Guitars,-38} ║", stats.TotalGuitars);
        _logger.LogInformation("║   • New Guitars Created: {Created,-39} ║", stats.GuitarsCreated);
        _logger.LogInformation("║   • Existing Guitars Updated: {Updated,-34} ║", stats.GuitarsUpdated);
        _logger.LogInformation("║   • Guitars Skipped: {Skipped,-43} ║", stats.GuitarsSkipped);
        _logger.LogInformation("║   • Processing Errors: {Errors,-41} ║", stats.Errors);
        _logger.LogInformation("║                                                               ║");
        _logger.LogInformation("║ Timing:                                                       ║");
        _logger.LogInformation("║   • Start Time: {Start,-48} ║", stats.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
        _logger.LogInformation("║   • End Time: {End,-50} ║", stats.EndTime.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
        _logger.LogInformation("║   • Duration: {Duration,-48} ║", $"{duration.Minutes}m {duration.Seconds}s");
        _logger.LogInformation("╚═══════════════════════════════════════════════════════════════╝");
        _logger.LogInformation("");
    }

    private class ScraperStats
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ApiCallsMade { get; set; }
        public int TotalListingsFetched { get; set; }
        public int TotalGuitars { get; set; }
        public int GuitarsProcessed { get; set; }
        public int GuitarsCreated { get; set; }
        public int GuitarsUpdated { get; set; }
        public int GuitarsSkipped { get; set; }
        public int Errors { get; set; }
    }
}
