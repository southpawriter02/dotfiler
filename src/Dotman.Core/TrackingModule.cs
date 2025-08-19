namespace Dotman.Core;

public class TrackingModule : ITrackingModule
{
    private readonly string _repositoryPath;
    private readonly ManifestService _manifestService;
    private readonly IVCSBackend _vcsBackend;

    public TrackingModule(string repositoryPath, ManifestService manifestService, IVCSBackend vcsBackend)
    {
        _repositoryPath = repositoryPath;
        _manifestService = manifestService;
        _vcsBackend = vcsBackend;
    }

    public async Task AddFileAsync(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (!fullPath.StartsWith(homePath))
        {
            throw new Exception("Tracked files must be within the user's home directory.");
        }

        var relativePath = Path.GetRelativePath(homePath, fullPath);
        var repoFilePath = Path.Combine(_repositoryPath, relativePath);

        var repoFileDir = Path.GetDirectoryName(repoFilePath);
        if (!string.IsNullOrEmpty(repoFileDir))
        {
            Directory.CreateDirectory(repoFileDir);
        }

        File.Copy(fullPath, repoFilePath, true);

        var entry = new DotfileEntry { Source = relativePath };
        _manifestService.AddEntry(relativePath, entry);
        await _manifestService.SaveAsync();

        // Backup original file before creating symlink
        var backupPath = fullPath + ".dotman-backup";
        File.Move(fullPath, backupPath);

        try
        {
            File.CreateSymbolicLink(fullPath, repoFilePath);
            File.Delete(backupPath); // Delete backup after successful symlink creation
        }
        catch
        {
            // If symlink fails, restore the backup
            File.Move(backupPath, fullPath);
            throw;
        }

        await _vcsBackend.CommitAsync($"Track new file: {relativePath}");
    }

    public async Task RemoveFileAsync(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var relativePath = Path.GetRelativePath(homePath, fullPath);

        if (!_manifestService.Manifest.Entries.TryGetValue(relativePath, out var entry))
        {
            throw new Exception($"File is not tracked: {relativePath}");
        }

        var repoFilePath = Path.Combine(_repositoryPath, entry.Source);

        File.Delete(fullPath); // Remove the symlink
        File.Copy(repoFilePath, fullPath); // Restore the original file

        File.Delete(repoFilePath);

        _manifestService.RemoveEntry(relativePath);
        await _manifestService.SaveAsync();

        await _vcsBackend.CommitAsync($"Stop tracking file: {relativePath}");
    }

    public Task<IEnumerable<string>> ListTrackedFilesAsync()
    {
        var trackedFiles = _manifestService.Manifest.Entries.Keys.AsEnumerable();
        return Task.FromResult(trackedFiles);
    }
}
