using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dotman.Gui.Services;

namespace Dotman.Gui.ViewModels;

/// <summary>
/// Settings view model for configuration management.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly IDotfileService _dotfileService;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _repositoryPath = string.Empty;

    [ObservableProperty]
    private string _currentProfile = "common";

    [ObservableProperty]
    private bool _autoSyncEnabled;

    [ObservableProperty]
    private int _autoSyncIntervalMinutes = 30;

    [ObservableProperty]
    private bool _showNotifications = true;

    [ObservableProperty]
    private bool _backupBeforeSync = true;

    [ObservableProperty]
    private string _gitUserName = string.Empty;

    [ObservableProperty]
    private string _gitUserEmail = string.Empty;

    [ObservableProperty]
    private string _remoteUrl = string.Empty;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _showStatus;

    [ObservableProperty]
    private string _statusClass = "info";

    [ObservableProperty]
    private bool _isInitialized;

    [ObservableProperty]
    private string _newRepoPath = string.Empty;

    [ObservableProperty]
    private bool _showInitDialog;

    // App info
    public string AppVersion => "1.0.0";
    public string DotNetVersion => Environment.Version.ToString();
    public string Platform => Environment.OSVersion.VersionString;

    public SettingsViewModel(IDotfileService dotfileService)
    {
        _dotfileService = dotfileService;
    }

    public async Task RefreshAsync()
    {
        IsLoading = true;

        try
        {
            await _dotfileService.InitializeAsync();
            IsInitialized = _dotfileService.IsInitialized;

            if (IsInitialized)
            {
                RepositoryPath = _dotfileService.RepositoryPath;
                CurrentProfile = _dotfileService.GetConfig("profile") ?? "common";

                // Load other settings if available
                var autoSync = _dotfileService.GetConfig("auto_sync");
                AutoSyncEnabled = autoSync?.ToLowerInvariant() == "true";

                var syncInterval = _dotfileService.GetConfig("sync_interval_minutes");
                if (int.TryParse(syncInterval, out var interval))
                    AutoSyncIntervalMinutes = interval;
            }

            HasUnsavedChanges = false;
        }
        catch
        {
            IsInitialized = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnRepositoryPathChanged(string value) => MarkAsChanged();
    partial void OnCurrentProfileChanged(string value) => MarkAsChanged();
    partial void OnAutoSyncEnabledChanged(bool value) => MarkAsChanged();
    partial void OnAutoSyncIntervalMinutesChanged(int value) => MarkAsChanged();
    partial void OnShowNotificationsChanged(bool value) => MarkAsChanged();
    partial void OnBackupBeforeSyncChanged(bool value) => MarkAsChanged();

    private void MarkAsChanged()
    {
        if (!IsLoading)
            HasUnsavedChanges = true;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            _dotfileService.SetConfig("profile", CurrentProfile);
            _dotfileService.SetConfig("auto_sync", AutoSyncEnabled.ToString().ToLowerInvariant());
            _dotfileService.SetConfig("sync_interval_minutes", AutoSyncIntervalMinutes.ToString());

            HasUnsavedChanges = false;
            ShowStatusMessage("Settings saved successfully", "success");
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Failed to save settings: {ex.Message}", "danger");
        }
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        await RefreshAsync();
        ShowStatusMessage("Settings reset to saved values", "info");
    }

    [RelayCommand]
    private void ShowInitializeDialog()
    {
        NewRepoPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dotfiles");
        ShowInitDialog = true;
    }

    [RelayCommand]
    private void CancelInitialize()
    {
        ShowInitDialog = false;
        NewRepoPath = string.Empty;
    }

    [RelayCommand]
    private async Task InitializeRepositoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRepoPath))
            return;

        try
        {
            await _dotfileService.InitializeRepositoryAsync(NewRepoPath);
            ShowInitDialog = false;
            await RefreshAsync();
            ShowStatusMessage("Repository initialized successfully", "success");
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Failed to initialize repository: {ex.Message}", "danger");
        }
    }

    [RelayCommand]
    private void BrowseForRepository()
    {
        // In a real implementation, this would open a folder browser dialog
        // For now, we'll just use a default path
        NewRepoPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dotfiles");
    }

    [RelayCommand]
    private void OpenRepositoryFolder()
    {
        if (!string.IsNullOrEmpty(RepositoryPath) && Directory.Exists(RepositoryPath))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = RepositoryPath,
                    UseShellExecute = true
                });
            }
            catch
            {
                ShowStatusMessage("Could not open folder", "warning");
            }
        }
    }

    [RelayCommand]
    private void OpenConfigFile()
    {
        var configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config", "dotman", "config.toml");

        if (File.Exists(configPath))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = configPath,
                    UseShellExecute = true
                });
            }
            catch
            {
                ShowStatusMessage("Could not open config file", "warning");
            }
        }
        else
        {
            ShowStatusMessage("Config file not found", "warning");
        }
    }

    private void ShowStatusMessage(string message, string statusClass, int durationMs = 3000)
    {
        StatusMessage = message;
        StatusClass = statusClass;
        ShowStatus = true;

        // Auto-hide after duration
        Task.Delay(durationMs).ContinueWith(_ =>
        {
            ShowStatus = false;
        });
    }
}
