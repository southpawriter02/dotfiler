using System.Text.Json;

namespace Dotman.Core;

public class ManifestService
{
    private readonly string _manifestPath;
    public Manifest Manifest { get; private set; } = new();

    public ManifestService(string repositoryPath)
    {
        _manifestPath = Path.Combine(repositoryPath, "manifest.json");
    }

    public async Task LoadOrCreateAsync()
    {
        if (!File.Exists(_manifestPath))
        {
            Manifest = new Manifest();
            await SaveAsync();
        }
        else
        {
            var json = await File.ReadAllTextAsync(_manifestPath);
            var manifestData = JsonSerializer.Deserialize<Dictionary<string, DotfileEntry>>(json);
            Manifest = new Manifest { Entries = manifestData ?? new() };
        }
    }

    public async Task SaveAsync()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(Manifest.Entries, options);
        await File.WriteAllTextAsync(_manifestPath, json);
    }

    public void AddEntry(string originalPath, DotfileEntry entry)
    {
        Manifest.Entries[originalPath] = entry;
    }

    public bool RemoveEntry(string originalPath)
    {
        return Manifest.Entries.Remove(originalPath);
    }
}
