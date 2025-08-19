namespace Dotman.Core;

public interface IConfigManager
{
    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value.</returns>
    string Get(string key);

    /// <summary>
    /// Sets a configuration value. (Note: Not fully implemented in this version).
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The new value.</param>
    void Set(string key, string value);

    /// <summary>
    /// Loads the configuration from the config file.
    /// </summary>
    Task LoadAsync();
}
