namespace Dotman.Core;

public interface ITrackingModule
{
    /// <summary>
    /// Starts tracking a new dotfile.
    /// </summary>
    /// <param name="filePath">The path to the file to track.</param>
    Task AddFileAsync(string filePath);

    /// <summary>
    /// Stops tracking a dotfile.
    /// </summary>
    /// <param name="filePath">The path to the file to stop tracking.</param>
    Task RemoveFileAsync(string filePath);

    /// <summary>
    /// Lists all currently tracked files.
    /// </summary>
    /// <returns>A collection of tracked file paths.</returns>
    Task<IEnumerable<string>> ListTrackedFilesAsync();
}
