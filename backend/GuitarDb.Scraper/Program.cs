using GuitarDb.Scraper.Configuration;
using GuitarDb.Scraper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
            optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        var mongoSettings = configuration.GetSection("MongoDB").Get<MongoDbSettings>();
        var reverbSettings = configuration.GetSection("ReverbApi").Get<ReverbApiSettings>();

        if (mongoSettings == null)
            throw new InvalidOperationException("MongoDB settings not found in configuration");
        if (reverbSettings == null)
            throw new InvalidOperationException("ReverbApi settings not found in configuration");

        if (string.IsNullOrEmpty(mongoSettings.ConnectionString))
            throw new InvalidOperationException("MongoDB ConnectionString is required");
        if (string.IsNullOrEmpty(reverbSettings.ApiKey) || reverbSettings.ApiKey == "YOUR_API_KEY_HERE")
            throw new InvalidOperationException("Reverb API Key is required. Please set it in appsettings.json or environment variables.");

        services.AddSingleton(mongoSettings);
        services.AddSingleton(reverbSettings);

        services.AddSingleton<GuitarRepository>();
        services.AddSingleton<ReverbApiClient>();
        services.AddSingleton<PriceAggregationService>();
        services.AddSingleton<ScraperOrchestrator>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

var orchestrator = host.Services.GetRequiredService<ScraperOrchestrator>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

// Parse command line arguments
var options = ScraperOptions.Parse(args);
var query = options.BuildQuery();

logger.LogInformation("Guitar Price Scraper");
logger.LogInformation("Parameters: {Options}", options);

// Show usage help if requested
if (args.Contains("--help") || args.Contains("-h"))
{
    PrintHelp();
    return 0;
}

try
{
    logger.LogInformation("Starting scraper with query: {Query}", query);
    await orchestrator.RunAsync(query);
    logger.LogInformation("Scraper completed successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Scraper failed with unhandled exception");
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine();
    Console.WriteLine("Guitar Price Scraper - Usage:");
    Console.WriteLine();
    Console.WriteLine("  dotnet run [brand] [options]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  brand              Brand to scrape (default: Gibson)");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --brand=<name>     Brand name (e.g., --brand=Fender)");
    Console.WriteLine("  --model=<name>     Specific model to scrape (e.g., --model='Les Paul')");
    Console.WriteLine("  --help, -h         Show this help message");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run");
    Console.WriteLine("  dotnet run Fender");
    Console.WriteLine("  dotnet run --brand=Gibson --model='Les Paul'");
    Console.WriteLine("  dotnet run --brand=Fender --model=Stratocaster");
    Console.WriteLine();
}
