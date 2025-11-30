using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dotman.Gui.Services;

namespace Dotman.Gui.ViewModels;

/// <summary>
/// Main window view model that manages navigation and overall app state.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDotfileService _dotfileService;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private string _currentViewName = "Dashboard";

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _statusMessage = "Initializing...";

    [ObservableProperty]
    private bool _isRepositoryInitialized;

    // Child view models
    public DashboardViewModel DashboardViewModel { get; }
    public FilesViewModel FilesViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public MainWindowViewModel(IDotfileService dotfileService)
    {
        _dotfileService = dotfileService;

        // Initialize child view models
        DashboardViewModel = new DashboardViewModel(dotfileService);
        FilesViewModel = new FilesViewModel(dotfileService);
        SettingsViewModel = new SettingsViewModel(dotfileService);

        // Set initial view
        CurrentView = DashboardViewModel;

        // Initialize async
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _dotfileService.InitializeAsync();
            IsRepositoryInitialized = _dotfileService.IsInitialized;

            if (IsRepositoryInitialized)
            {
                StatusMessage = "Ready";
                await DashboardViewModel.RefreshAsync();
            }
            else
            {
                StatusMessage = "No repository configured";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentView = DashboardViewModel;
        CurrentViewName = "Dashboard";
        _ = DashboardViewModel.RefreshAsync();
    }

    [RelayCommand]
    private void NavigateToFiles()
    {
        CurrentView = FilesViewModel;
        CurrentViewName = "Files";
        _ = FilesViewModel.RefreshAsync();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentView = SettingsViewModel;
        CurrentViewName = "Settings";
        _ = SettingsViewModel.RefreshAsync();
    }

    [RelayCommand]
    private async Task SyncAsync()
    {
        if (!IsRepositoryInitialized)
            return;

        try
        {
            StatusMessage = "Syncing...";
            await _dotfileService.SyncAsync();
            StatusMessage = "Sync complete";

            // Refresh current view
            if (CurrentView == DashboardViewModel)
                await DashboardViewModel.RefreshAsync();
            else if (CurrentView == FilesViewModel)
                await FilesViewModel.RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Sync failed: {ex.Message}";
        }
    }
}
