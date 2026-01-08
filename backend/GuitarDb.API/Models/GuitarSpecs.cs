using MongoDB.Bson.Serialization.Attributes;

namespace GuitarDb.API.Models;

public class GuitarSpecs
{
    [BsonElement("body")]
    public BodySpecs? Body { get; set; }

    [BsonElement("neck")]
    public NeckSpecs? Neck { get; set; }

    [BsonElement("electronics")]
    public ElectronicsSpecs? Electronics { get; set; }

    [BsonElement("hardware")]
    public HardwareSpecs? Hardware { get; set; }
}

public class BodySpecs
{
    [BsonElement("wood")]
    public string? Wood { get; set; }

    [BsonElement("top")]
    public string? Top { get; set; }

    [BsonElement("binding")]
    public string? Binding { get; set; }
}

public class NeckSpecs
{
    [BsonElement("wood")]
    public string? Wood { get; set; }

    [BsonElement("profile")]
    public string? Profile { get; set; }

    [BsonElement("frets")]
    public int? Frets { get; set; }

    [BsonElement("scaleLength")]
    public double? ScaleLength { get; set; }
}

public class ElectronicsSpecs
{
    [BsonElement("pickups")]
    public List<string>? Pickups { get; set; }

    [BsonElement("controls")]
    public List<string>? Controls { get; set; }
}

public class HardwareSpecs
{
    [BsonElement("bridge")]
    public string? Bridge { get; set; }

    [BsonElement("tailpiece")]
    public string? Tailpiece { get; set; }

    [BsonElement("tuners")]
    public string? Tuners { get; set; }
}
