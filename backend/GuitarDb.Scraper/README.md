# Guitar Price Scraper

A console application that scrapes guitar pricing data from Reverb API, aggregates statistics, and stores historical snapshots in MongoDB.

## Features

- 🎸 **Brand & Model Filtering**: Search for specific brands or model combinations
- 📊 **Price Statistics**: Calculate avg/min/max prices for all 6 condition levels
- 💾 **MongoDB Storage**: Store guitars with historical price snapshots
- 🔄 **Pagination Support**: Automatically handles Reverb API pagination
- 🛡️ **Resilience**: Built-in retry logic with exponential backoff and circuit breaker
- 📈 **Detailed Reporting**: Comprehensive summary with API statistics and timing
- ⚙️ **Configurable**: Easy configuration via appsettings.json or environment variables

## Prerequisites

- .NET 9.0 SDK
- MongoDB (local or remote)
- Reverb API Personal Access Token

## Configuration

Edit `appsettings.json`:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "GuitarDb",
    "GuitarsCollectionName": "guitars"
  },
  "ReverbApi": {
    "ApiKey": "YOUR_API_KEY_HERE",
    "BaseUrl": "https://api.reverb.com/api",
    "PageSize": 50,
    "MaxRetries": 3,
    "RetryDelayMs": 1000,
    "RateLimitDelayMs": 500
  }
}
```

### Environment Variables (Production)

```bash
export MongoDB__ConnectionString="mongodb+srv://your-cluster..."
export ReverbApi__ApiKey="your_api_key_here"
```

## Usage

### Basic Usage

```bash
# Scrape all Gibson guitars (default)
dotnet run

# Scrape a different brand
dotnet run Fender

# Scrape specific model
dotnet run --brand=Gibson --model="Les Paul"
```

### Command Line Arguments

```bash
dotnet run [brand] [options]
```

**Positional Arguments:**
- `brand` - Brand name to scrape (default: Gibson)

**Options:**
- `--brand=<name>` - Specify brand explicitly
- `--model=<name>` - Filter by specific model (use quotes for multi-word models)
- `--help`, `-h` - Show help message

### Examples

```bash
# Search for all Gibson guitars
dotnet run

# Search for Fender guitars
dotnet run Fender

# Search for Gibson Les Paul models only
dotnet run --brand=Gibson --model="Les Paul"

# Search for Fender Stratocasters
dotnet run --brand=Fender --model=Stratocaster

# Search for PRS Custom 24 models
dotnet run --brand=PRS --model="Custom 24"
```

## Output Example

```
===== Starting Guitar Price Scraper =====
Query: Gibson Les Paul
Start Time: 2026-01-08 14:30:00 UTC

Step 1: Fetching listings from Reverb API...
Fetched page 1: 50 listings (48 live, 48 total so far)
Fetched page 2: 50 listings (47 live, 95 total so far)
Fetched page 3: 35 listings (35 live, 130 total so far)

Step 2: Grouping listings by guitar (Make + Model + Year)...
Grouped 130 listings into 42 unique guitars

Step 3: Processing 42 unique guitars...
[1/42] Processing: Gibson Les Paul Standard (2020)
  - Calculated prices for 6 conditions (12 listings)
  - Created new guitar document
[2/42] Processing: Gibson Les Paul Traditional (2019)
  - Calculated prices for 6 conditions (8 listings)
  - Appended snapshot to existing guitar

╔═══════════════════════════════════════════════════════════════╗
║              GUITAR PRICE SCRAPER SUMMARY                     ║
╠═══════════════════════════════════════════════════════════════╣
║ API Statistics:                                               ║
║   • API Calls Made: 3                                         ║
║   • Total Listings Fetched: 130                               ║
║                                                               ║
║ Guitar Statistics:                                            ║
║   • Unique Guitars Found: 42                                  ║
║   • New Guitars Created: 28                                   ║
║   • Existing Guitars Updated: 14                              ║
║   • Guitars Skipped: 0                                        ║
║   • Processing Errors: 0                                      ║
║                                                               ║
║ Timing:                                                       ║
║   • Start Time: 2026-01-08 14:30:00 UTC                       ║
║   • End Time: 2026-01-08 14:31:23 UTC                         ║
║   • Duration: 1m 23s                                          ║
╚═══════════════════════════════════════════════════════════════╝
```

## MongoDB Data Structure

Guitars are stored with embedded price history:

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "make": "Gibson",
  "model": "Les Paul Standard",
  "year": 2020,
  "category": "Electric",
  "priceHistory": [
    {
      "date": "2026-01-08T00:00:00Z",
      "scrapedAt": "2026-01-08T14:30:00Z",
      "totalListingsScraped": 45,
      "conditionPricing": [
        {
          "condition": "Mint",
          "averagePrice": 2499.50,
          "minPrice": 2200.00,
          "maxPrice": 2799.00,
          "listingCount": 12,
          "currency": "USD"
        },
        {
          "condition": "Excellent",
          "averagePrice": 2199.75,
          "minPrice": 1950.00,
          "maxPrice": 2450.00,
          "listingCount": 18,
          "currency": "USD"
        }
        // ... 4 more conditions
      ]
    }
  ],
  "createdAt": "2026-01-08T14:30:00Z",
  "updatedAt": "2026-01-08T14:30:00Z"
}
```

## Deduplication

The scraper automatically prevents duplicate guitar entries:

- Guitars are uniquely identified by: **Make + Model + Year**
- When processing listings, it checks if a guitar already exists in MongoDB
- If found: Appends new PriceSnapshot to existing history
- If not found: Creates new Guitar document with initial snapshot

## Scheduling

### Linux/MacOS (cron)

```bash
# Run daily at 2 AM
0 2 * * * cd /path/to/GuitarDb.Scraper && dotnet run >> scraper.log 2>&1
```

### Windows (Task Scheduler)

1. Open Task Scheduler
2. Create Basic Task
3. Set Trigger: Daily at 2:00 AM
4. Action: Start a program
   - Program: `dotnet.exe`
   - Arguments: `run --project C:\path\to\GuitarDb.Scraper`
5. Redirect output to log file if needed

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "GuitarDb.Scraper.dll"]
```

## Error Handling

- **API Failures**: Automatic retry with exponential backoff (3 attempts)
- **Rate Limiting**: Respects 429 responses, implements rate limit delays
- **Circuit Breaker**: Stops requests if API is consistently failing
- **Per-Guitar Errors**: Continues processing other guitars if one fails
- **Comprehensive Logging**: All errors logged with context

## Architecture

```
┌─────────────────┐
│   Program.cs    │ Entry point, DI setup, Polly policies
└────────┬────────┘
         │
┌────────▼───────────────┐
│ ScraperOrchestrator    │ Main workflow coordination
└────────┬───────────────┘
         │
         ├─────────────────────┐
         │                     │
┌────────▼─────────┐  ┌───────▼──────────────┐
│ ReverbApiClient  │  │ GuitarRepository     │
│ - Pagination     │  │ - Find/Create        │
│ - Retry logic    │  │ - Append snapshots   │
│ - Rate limiting  │  │ - MongoDB operations │
└──────────────────┘  └──────────────────────┘
         │
┌────────▼─────────────────┐
│ PriceAggregationService  │
│ - Group by Make+Model    │
│ - Calculate statistics   │
└──────────────────────────┘
```

## Development

### Build

```bash
dotnet build
```

### Run Locally

```bash
# Start MongoDB
docker run -d -p 27017:27017 --name mongodb mongo:latest

# Run scraper
dotnet run --brand=Gibson
```

### Testing Different Scenarios

```bash
# Test with model filter
dotnet run --brand=Gibson --model="SG Special"

# Test with different brand
dotnet run Fender

# Test help
dotnet run -- --help
```

## Troubleshooting

### "MongoDB connection failed"

- Verify MongoDB is running: `mongosh` or `docker ps`
- Check connection string in appsettings.json
- Ensure firewall allows port 27017

### "Reverb API Key is required"

- Verify API key is set in appsettings.json
- Key should not be "YOUR_API_KEY_HERE"
- Get key from: https://reverb.com/my/account/api

### "No listings found"

- Verify brand/model spelling
- Check Reverb.com to confirm listings exist
- Try broader search (brand only, no model filter)

## License

MIT
