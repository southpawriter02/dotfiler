using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dotman.Gui.Services;

namespace Dotman.Gui.ViewModels;

/// <summary>
/// Dashboard view model showing status overview and quick actions.
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDotfileService _dotfileService;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _isInitialized;

    [ObservableProperty]
    private int _totalFiles;

    [ObservableProperty]
    private int _modifiedFiles;

    [ObservableProperty]
    private int _syncedFiles;

    [ObservableProperty]
    private int _missingFiles;

    [ObservableProperty]
    private string _syncStatus = "Unknown";

    [ObservableProperty]
    private string _syncStatusClass = "info";

    [ObservableProperty]
    private string _lastSyncTime = "Never";

    [ObservableProperty]
    private string _repositoryPath = string.Empty;

    [ObservableProperty]
    private string _currentProfile = "common";

    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    // Quick stats for dashboard cards
    [ObservableProperty]
    private string _healthStatus = "Good";

    [ObservableProperty]
    private string _healthStatusClass = "success";

    [ObservableProperty]
    private double _syncProgress = 100;

    public DashboardViewModel(IDotfileService dotfileService)
    {
        _dotfileService = dotfileService;
    }

    public async Task RefreshAsync()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var status = await _dotfileService.GetStatusAsync();

            IsInitialized = status.IsInitialized;
            TotalFiles = status.TotalFiles;
            ModifiedFiles = status.ModifiedFiles;
            MissingFiles = status.MissingFiles;
            SyncedFiles = TotalFiles - ModifiedFiles - MissingFiles;
            RepositoryPath = status.RepositoryPath;

            // Determine sync status display
            switch (status.SyncState)
            {
                case SyncState.Synced:
                    SyncStatus = "All Synced";
                    SyncStatusClass = "success";
                    break;
                case SyncState.HasChanges:
                    SyncStatus = "Changes Pending";
                    SyncStatusClass = "warning";
                    break;
                case SyncState.Syncing:
                    SyncStatus = "Syncing...";
                    SyncStatusClass = "info";
                    break;
                case SyncState.Error:
                    SyncStatus = "Error";
                    SyncStatusClass = "danger";
                    break;
            }

            // Last sync time
            if (status.LastSyncTime.HasValue)
            {
                var elapsed = DateTime.UtcNow - status.LastSyncTime.Value;
                LastSyncTime = FormatElapsedTime(elapsed);
            }
            else
            {
                LastSyncTime = "Never";
            }

            // Calculate health status
            UpdateHealthStatus();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            SyncStatus = "Error";
            SyncStatusClass = "danger";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateHealthStatus()
    {
        if (!IsInitialized)
        {
            HealthStatus = "Not Configured";
            HealthStatusClass = "warning";
            SyncProgress = 0;
            return;
        }

        if (TotalFiles == 0)
        {
            HealthStatus = "No Files";
            HealthStatusClass = "info";
            SyncProgress = 100;
            return;
        }

        var healthPercentage = (double)SyncedFiles / TotalFiles * 100;
        SyncProgress = healthPercentage;

        if (healthPercentage >= 90)
        {
            HealthStatus = "Excellent";
            HealthStatusClass = "success";
        }
        else if (healthPercentage >= 70)
        {
            HealthStatus = "Good";
            HealthStatusClass = "success";
        }
        else if (healthPercentage >= 50)
        {
            HealthStatus = "Fair";
            HealthStatusClass = "warning";
        }
        else
        {
            HealthStatus = "Needs Attention";
            HealthStatusClass = "danger";
        }
    }

    [RelayCommand]
    private async Task SyncNowAsync()
    {
        if (IsSyncing || !IsInitialized)
            return;

        IsSyncing = true;
        SyncStatus = "Syncing...";
        SyncStatusClass = "info";

        try
        {
            await _dotfileService.SyncAsync();
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Sync failed: {ex.Message}";
            SyncStatus = "Sync Failed";
            SyncStatusClass = "danger";
        }
        finally
        {
            IsSyncing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshStatusAsync()
    {
        await RefreshAsync();
    }

    private static string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalMinutes < 1)
            return "Just now";
        if (elapsed.TotalMinutes < 60)
            return $"{(int)elapsed.TotalMinutes} min ago";
        if (elapsed.TotalHours < 24)
            return $"{(int)elapsed.TotalHours} hours ago";
        if (elapsed.TotalDays < 7)
            return $"{(int)elapsed.TotalDays} days ago";
        return $"{(int)(elapsed.TotalDays / 7)} weeks ago";
    }
}
