using System.Text.Json.Serialization;

namespace Dotman.Core;

public class DotfileEntry
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("profile")]
    public string Profile { get; set; } = "common";

    [JsonPropertyName("is_template")]
    public bool IsTemplate { get; set; }

    [JsonPropertyName("is_secret")]
    public bool IsSecret { get; set; }
}

public class Manifest
{
    public Dictionary<string, DotfileEntry> Entries { get; set; } = new();
}
