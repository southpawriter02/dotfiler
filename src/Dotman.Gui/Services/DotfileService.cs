using System.Collections.ObjectModel;
using Dotman.Core;

namespace Dotman.Gui.Services;

/// <summary>
/// Service that wraps the Dotman.Core library for use in the GUI.
/// Provides a clean interface for all dotfile management operations.
/// </summary>
public class DotfileService : IDotfileService
{
    private readonly IConfigManager _configManager;
    private readonly IVCSBackend _gitBackend;
    private readonly ITrackingModule _trackingModule;
    private readonly ISyncModule _syncModule;
    private readonly ManifestService _manifestService;

    private string _repoPath = string.Empty;
    private bool _isInitialized;

    public bool IsInitialized => _isInitialized;
    public string RepositoryPath => _repoPath;

    public DotfileService()
    {
        _configManager = new ConfigManager();
        _gitBackend = new GitBackend(() => _repoPath);
        _manifestService = new ManifestService(() => _repoPath);
        _trackingModule = new TrackingModule(_manifestService, _gitBackend, () => _repoPath);
        _syncModule = new SyncModule(_gitBackend);
    }

    /// <summary>
    /// Initializes the service by loading configuration.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            await _configManager.LoadAsync();
            _repoPath = _configManager.Get("repo_path");
            _isInitialized = !string.IsNullOrEmpty(_repoPath) && Directory.Exists(_repoPath);
        }
        catch
        {
            _isInitialized = false;
        }
    }

    /// <summary>
    /// Initializes a new dotfiles repository at the specified path.
    /// </summary>
    public async Task InitializeRepositoryAsync(string path)
    {
        _repoPath = path;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        await _gitBackend.InitAsync();
        _configManager.Set("repo_path", path);
        _isInitialized = true;
    }

    /// <summary>
    /// Gets all tracked dotfiles.
    /// </summary>
    public async Task<IEnumerable<TrackedFileInfo>> GetTrackedFilesAsync()
    {
        if (!_isInitialized)
            return Enumerable.Empty<TrackedFileInfo>();

        var manifest = await _manifestService.LoadManifestAsync();
        var files = new List<TrackedFileInfo>();

        foreach (var entry in manifest.Entries)
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var originalPath = Path.Combine(homeDir, entry.Value.Source);
            var repoFilePath = Path.Combine(_repoPath, entry.Key);

            var status = FileStatus.Synced;
            var linkExists = File.Exists(originalPath) || Directory.Exists(originalPath);

            if (!linkExists)
            {
                status = FileStatus.Missing;
            }
            else if (File.Exists(repoFilePath))
            {
                // Check if the file has been modified
                var linkInfo = new FileInfo(originalPath);
                var repoInfo = new FileInfo(repoFilePath);

                if (linkInfo.LastWriteTimeUtc > repoInfo.LastWriteTimeUtc)
                {
                    status = FileStatus.Modified;
                }
            }

            files.Add(new TrackedFileInfo
            {
                FileName = entry.Key,
                SourcePath = entry.Value.Source,
                FullPath = originalPath,
                RepositoryPath = repoFilePath,
                Profile = entry.Value.Profile,
                IsTemplate = entry.Value.IsTemplate,
                IsSecret = entry.Value.IsSecret,
                Status = status
            });
        }

        return files;
    }

    /// <summary>
    /// Adds a file to tracking.
    /// </summary>
    public async Task AddFileAsync(string filePath)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Repository not initialized");

        await _trackingModule.AddFileAsync(filePath);
    }

    /// <summary>
    /// Removes a file from tracking.
    /// </summary>
    public async Task RemoveFileAsync(string filePath)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Repository not initialized");

        await _trackingModule.RemoveFileAsync(filePath);
    }

    /// <summary>
    /// Synchronizes with remote repository.
    /// </summary>
    public async Task SyncAsync()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Repository not initialized");

        await _syncModule.SyncAsync();
    }

    /// <summary>
    /// Gets the repository status.
    /// </summary>
    public async Task<RepositoryStatus> GetStatusAsync()
    {
        if (!_isInitialized)
        {
            return new RepositoryStatus
            {
                IsInitialized = false,
                StatusMessage = "No repository configured"
            };
        }

        try
        {
            var gitStatus = await _gitBackend.StatusAsync();
            var files = await GetTrackedFilesAsync();
            var fileList = files.ToList();

            var modifiedCount = fileList.Count(f => f.Status == FileStatus.Modified);
            var missingCount = fileList.Count(f => f.Status == FileStatus.Missing);

            SyncState syncState;
            string statusMessage;

            if (string.IsNullOrWhiteSpace(gitStatus))
            {
                syncState = SyncState.Synced;
                statusMessage = "All files are synced";
            }
            else if (gitStatus.Contains("nothing to commit"))
            {
                syncState = SyncState.Synced;
                statusMessage = "Working tree clean";
            }
            else
            {
                syncState = SyncState.HasChanges;
                statusMessage = $"{modifiedCount} modified, {missingCount} missing";
            }

            return new RepositoryStatus
            {
                IsInitialized = true,
                TotalFiles = fileList.Count,
                ModifiedFiles = modifiedCount,
                MissingFiles = missingCount,
                SyncState = syncState,
                StatusMessage = statusMessage,
                LastSyncTime = GetLastCommitTime(),
                RepositoryPath = _repoPath
            };
        }
        catch (Exception ex)
        {
            return new RepositoryStatus
            {
                IsInitialized = true,
                SyncState = SyncState.Error,
                StatusMessage = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Gets the configuration value for a key.
    /// </summary>
    public string GetConfig(string key)
    {
        return _configManager.Get(key);
    }

    /// <summary>
    /// Sets a configuration value.
    /// </summary>
    public void SetConfig(string key, string value)
    {
        _configManager.Set(key, value);
    }

    private DateTime? GetLastCommitTime()
    {
        try
        {
            var gitDir = Path.Combine(_repoPath, ".git");
            if (Directory.Exists(gitDir))
            {
                var headFile = Path.Combine(gitDir, "HEAD");
                if (File.Exists(headFile))
                {
                    return File.GetLastWriteTimeUtc(headFile);
                }
            }
        }
        catch
        {
            // Ignore errors
        }
        return null;
    }
}

/// <summary>
/// Interface for the dotfile service.
/// </summary>
public interface IDotfileService
{
    bool IsInitialized { get; }
    string RepositoryPath { get; }

    Task InitializeAsync();
    Task InitializeRepositoryAsync(string path);
    Task<IEnumerable<TrackedFileInfo>> GetTrackedFilesAsync();
    Task AddFileAsync(string filePath);
    Task RemoveFileAsync(string filePath);
    Task SyncAsync();
    Task<RepositoryStatus> GetStatusAsync();
    string GetConfig(string key);
    void SetConfig(string key, string value);
}

/// <summary>
/// Information about a tracked file.
/// </summary>
public class TrackedFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string RepositoryPath { get; set; } = string.Empty;
    public string Profile { get; set; } = "common";
    public bool IsTemplate { get; set; }
    public bool IsSecret { get; set; }
    public FileStatus Status { get; set; }

    public string StatusDisplay => Status switch
    {
        FileStatus.Synced => "Synced",
        FileStatus.Modified => "Modified",
        FileStatus.Missing => "Missing",
        FileStatus.New => "New",
        _ => "Unknown"
    };
}

/// <summary>
/// File tracking status.
/// </summary>
public enum FileStatus
{
    Synced,
    Modified,
    Missing,
    New
}

/// <summary>
/// Repository status information.
/// </summary>
public class RepositoryStatus
{
    public bool IsInitialized { get; set; }
    public int TotalFiles { get; set; }
    public int ModifiedFiles { get; set; }
    public int MissingFiles { get; set; }
    public SyncState SyncState { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public DateTime? LastSyncTime { get; set; }
    public string RepositoryPath { get; set; } = string.Empty;
}

/// <summary>
/// Sync state enumeration.
/// </summary>
public enum SyncState
{
    Synced,
    HasChanges,
    Syncing,
    Error
}
