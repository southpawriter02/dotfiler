using Tomlyn;
using Tomlyn.Model;

namespace Dotman.Core;

public class Config
{
    public string? RepositoryPath { get; set; }
}

public class ConfigManager : IConfigManager
{
    private Config _config = new();
    private string? _configPath;

    public async Task LoadAsync()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _configPath = Path.Combine(userProfile, ".config", "dotman", "config.toml");

        if (!File.Exists(_configPath))
        {
            // Set default values if the config file doesn't exist.
            _config = new Config
            {
                RepositoryPath = Path.Combine(userProfile, "dotfiles")
            };
            return;
        }

        var content = await File.ReadAllTextAsync(_configPath);
        _config = Toml.ToModel<Config>(content);
    }

    public string Get(string key)
    {
        return key.ToLower() switch
        {
            "repositorypath" => _config.RepositoryPath ?? string.Empty,
            _ => throw new ArgumentException($"Configuration key '{key}' not found.")
        };
    }

    public void Set(string key, string value)
    {
        // For now, this is a read-only implementation.
        // A full implementation would modify the in-memory config
        // and provide a SaveAsync method.
        throw new NotImplementedException();
    }
}
