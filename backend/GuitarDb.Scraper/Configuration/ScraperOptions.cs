namespace GuitarDb.Scraper.Configuration;

public class ScraperOptions
{
    public string Brand { get; set; } = "Gibson";
    public string? Model { get; set; }

    public static ScraperOptions Parse(string[] args)
    {
        var options = new ScraperOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("--brand=", StringComparison.OrdinalIgnoreCase))
            {
                options.Brand = arg.Substring("--brand=".Length).Trim();
            }
            else if (arg.StartsWith("--model=", StringComparison.OrdinalIgnoreCase))
            {
                options.Model = arg.Substring("--model=".Length).Trim().Trim('\'', '"');
            }
            else if (i == 0 && !arg.StartsWith("--"))
            {
                // First positional argument is brand
                options.Brand = arg;
            }
        }

        return options;
    }

    public string BuildQuery()
    {
        if (!string.IsNullOrEmpty(Model))
        {
            return $"{Brand} {Model}";
        }
        return Brand;
    }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Model))
        {
            return $"Brand: {Brand}, Model: {Model}";
        }
        return $"Brand: {Brand}";
    }
}
