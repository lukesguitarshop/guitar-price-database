using System.Diagnostics;
using System.Text.Json;
using GuitarDb.Scraper.Configuration;
using GuitarDb.Scraper.Models.Reverb;
using Microsoft.Extensions.Logging;

namespace GuitarDb.Scraper.Services;

public class ReverbApiClient
{
    private readonly ILogger<ReverbApiClient> _logger;
    private readonly ReverbApiSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReverbApiClient(
        ReverbApiSettings settings,
        ILogger<ReverbApiClient> logger)
    {
        _settings = settings;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private async Task<string> ExecuteCurlAsync(string url, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "curl",
            Arguments = $"-s -H \"Authorization: Bearer {_settings.ApiKey}\" -H \"Accept: application/hal+json\" -H \"Accept-Version: 3.0\" \"{url}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _logger.LogError("cURL failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
            throw new HttpRequestException($"cURL request failed: {error}");
        }

        return output;
    }

    public async Task<SearchResult> SearchGuitarsAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var allListings = new List<ReverbListing>();
        var currentPage = 1;
        string? nextUrl = $"{_settings.BaseUrl}/listings?query={Uri.EscapeDataString(query)}&per_page={_settings.PageSize}";

        _logger.LogInformation("Starting search for: {Query}", query);

        while (!string.IsNullOrEmpty(nextUrl))
        {
            try
            {
                _logger.LogDebug("Fetching page {Page}: {Url}", currentPage, nextUrl);

                var content = await ExecuteCurlAsync(nextUrl, cancellationToken);

                var reverbResponse = JsonSerializer.Deserialize<ReverbListingsResponse>(content, _jsonOptions);

                if (reverbResponse == null || reverbResponse.Listings == null)
                {
                    _logger.LogWarning("Received null response from Reverb API");
                    break;
                }

                var liveListings = reverbResponse.Listings
                    .Where(l => l.State.Slug.Equals("live", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                allListings.AddRange(liveListings);

                _logger.LogInformation("Fetched page {Page}: {Count} listings ({Live} live, {Total} total so far)",
                    currentPage, reverbResponse.Listings.Count, liveListings.Count, allListings.Count);

                // Extract next URL from HAL links (will be full URL)
                nextUrl = reverbResponse.Links?.Next?.Href;

                if (!string.IsNullOrEmpty(nextUrl))
                {
                    _logger.LogDebug("Rate limiting: waiting {Delay}ms before next request", _settings.RateLimitDelayMs);
                    await Task.Delay(_settings.RateLimitDelayMs, cancellationToken);
                }

                currentPage++;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching page {Page}", currentPage);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse JSON response from page {Page}", currentPage);
                throw;
            }
        }

        _logger.LogInformation("Completed search: fetched {Total} live listings across {Pages} pages",
            allListings.Count, currentPage - 1);

        return new SearchResult
        {
            Listings = allListings,
            ApiCallsMade = currentPage - 1,
            TotalListingsFetched = allListings.Count
        };
    }
}

public class SearchResult
{
    public List<ReverbListing> Listings { get; set; } = new();
    public int ApiCallsMade { get; set; }
    public int TotalListingsFetched { get; set; }
}
