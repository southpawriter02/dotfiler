using Dotman.Core;

namespace Dotman.Core.Tests;

public class ConfigManagerTests
{
    [Fact]
    public async Task LoadAsync_WhenConfigFileExists_LoadsRepositoryPath()
    {
        // Arrange
        var tempHome = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var dotmanConfigDir = Path.Combine(tempHome, ".config", "dotman");
        Directory.CreateDirectory(dotmanConfigDir);
        var configPath = Path.Combine(dotmanConfigDir, "config.toml");
        var expectedRepoPath = "/test/path/to/repo";
        await File.WriteAllTextAsync(configPath, $"repository_path = \"{expectedRepoPath}\"");

        // Temporarily set the HOME environment variable for the test
        var originalHome = Environment.GetEnvironmentVariable("HOME");
        var originalUserProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        Environment.SetEnvironmentVariable("HOME", tempHome);
        Environment.SetEnvironmentVariable("USERPROFILE", tempHome);

        try
        {
            var configManager = new ConfigManager();

            // Act
            await configManager.LoadAsync();
            var actualRepoPath = configManager.Get("repositorypath");

            // Assert
            Assert.Equal(expectedRepoPath, actualRepoPath);
        }
        finally
        {
            // Clean up
            Environment.SetEnvironmentVariable("HOME", originalHome);
            Environment.SetEnvironmentVariable("USERPROFILE", originalUserProfile);
            Directory.Delete(tempHome, true);
        }
    }
}
