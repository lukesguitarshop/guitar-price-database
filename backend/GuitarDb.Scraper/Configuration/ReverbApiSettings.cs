namespace GuitarDb.Scraper.Configuration;

public class ReverbApiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.reverb.com/api";
    public int PageSize { get; set; } = 50;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
    public int RateLimitDelayMs { get; set; } = 500;
}
