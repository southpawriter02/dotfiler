namespace Dotman.Core;

public interface IVCSBackend
{
    /// <summary>
    /// Initializes a new version control repository.
    /// </summary>
    Task InitAsync();

    /// <summary>
    /// Commits all staged changes with the given message.
    /// </summary>
    /// <param name="message">The commit message.</param>
    Task CommitAsync(string message);

    /// <summary>
    /// Pulls the latest changes from the remote repository.
    /// </summary>
    Task PullAsync();

    /// <summary>
    /// Pushes local changes to the remote repository.
    /// </summary>
    Task PushAsync();

    /// <summary>
    /// Gets the current status of the repository.
    /// </summary>
    /// <returns>A string representing the repository status.</returns>
    Task<string> StatusAsync();

    /// <summary>
    /// Clones a remote repository to a local path.
    /// </summary>
    /// <param name="repoUrl">The URL of the remote repository.</param>
    /// <param name="localPath">The local path to clone into.</param>
    Task CloneAsync(string repoUrl, string localPath);
}
