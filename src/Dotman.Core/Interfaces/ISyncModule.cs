namespace Dotman.Core;

public interface ISyncModule
{
    /// <summary>
    /// Synchronizes the local repository with the remote.
    /// </summary>
    Task SyncAsync();

    /// <summary>
    /// Gets the status of the local repository compared to the remote.
    /// </summary>
    /// <returns>A string representing the repository status.</returns>
    Task<string> GetRemoteStatusAsync();
}
