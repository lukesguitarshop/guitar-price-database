namespace GuitarDb.Scraper.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "GuitarDb";
    public string GuitarsCollectionName { get; set; } = "guitars";
}
