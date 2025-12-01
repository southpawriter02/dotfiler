using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dotman.Gui.Services;

namespace Dotman.Gui.ViewModels;

/// <summary>
/// Files view model for managing tracked dotfiles.
/// </summary>
public partial class FilesViewModel : ViewModelBase
{
    private readonly IDotfileService _dotfileService;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private ObservableCollection<FileItemViewModel> _files = new();

    [ObservableProperty]
    private FileItemViewModel? _selectedFile;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _filterStatus = "All";

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _filteredCount;

    [ObservableProperty]
    private bool _showAddFileDialog;

    [ObservableProperty]
    private string _newFilePath = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isProcessing;

    private List<FileItemViewModel> _allFiles = new();

    public FilesViewModel(IDotfileService dotfileService)
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
            var trackedFiles = await _dotfileService.GetTrackedFilesAsync();

            _allFiles = trackedFiles.Select(f => new FileItemViewModel(f)).ToList();
            TotalCount = _allFiles.Count;

            ApplyFilters();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnFilterStatusChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allFiles.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(f =>
                f.FileName.ToLowerInvariant().Contains(searchLower) ||
                f.SourcePath.ToLowerInvariant().Contains(searchLower));
        }

        // Apply status filter
        if (FilterStatus != "All")
        {
            filtered = FilterStatus switch
            {
                "Synced" => filtered.Where(f => f.Status == FileStatus.Synced),
                "Modified" => filtered.Where(f => f.Status == FileStatus.Modified),
                "Missing" => filtered.Where(f => f.Status == FileStatus.Missing),
                _ => filtered
            };
        }

        Files = new ObservableCollection<FileItemViewModel>(filtered);
        FilteredCount = Files.Count;
    }

    [RelayCommand]
    private void ShowAddFile()
    {
        ShowAddFileDialog = true;
        NewFilePath = string.Empty;
    }

    [RelayCommand]
    private void CancelAddFile()
    {
        ShowAddFileDialog = false;
        NewFilePath = string.Empty;
    }

    [RelayCommand]
    private async Task AddFileAsync()
    {
        if (string.IsNullOrWhiteSpace(NewFilePath))
            return;

        IsProcessing = true;
        HasError = false;

        try
        {
            await _dotfileService.AddFileAsync(NewFilePath);
            ShowAddFileDialog = false;
            NewFilePath = string.Empty;
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to add file: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task RemoveFileAsync(FileItemViewModel? file)
    {
        if (file == null)
            return;

        IsProcessing = true;
        HasError = false;

        try
        {
            await _dotfileService.RemoveFileAsync(file.FullPath);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to remove file: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void SetFilterAll() => FilterStatus = "All";

    [RelayCommand]
    private void SetFilterSynced() => FilterStatus = "Synced";

    [RelayCommand]
    private void SetFilterModified() => FilterStatus = "Modified";

    [RelayCommand]
    private void SetFilterMissing() => FilterStatus = "Missing";
}

/// <summary>
/// View model for a single tracked file.
/// </summary>
public partial class FileItemViewModel : ViewModelBase
{
    public string FileName { get; }
    public string SourcePath { get; }
    public string FullPath { get; }
    public string RepositoryPath { get; }
    public string Profile { get; }
    public bool IsTemplate { get; }
    public bool IsSecret { get; }
    public FileStatus Status { get; }

    public string StatusDisplay => Status switch
    {
        FileStatus.Synced => "Synced",
        FileStatus.Modified => "Modified",
        FileStatus.Missing => "Missing",
        FileStatus.New => "New",
        _ => "Unknown"
    };

    public string StatusClass => Status switch
    {
        FileStatus.Synced => "success",
        FileStatus.Modified => "warning",
        FileStatus.Missing => "danger",
        FileStatus.New => "info",
        _ => "muted"
    };

    public string FileIcon => GetFileIcon();

    public FileItemViewModel(TrackedFileInfo fileInfo)
    {
        FileName = fileInfo.FileName;
        SourcePath = fileInfo.SourcePath;
        FullPath = fileInfo.FullPath;
        RepositoryPath = fileInfo.RepositoryPath;
        Profile = fileInfo.Profile;
        IsTemplate = fileInfo.IsTemplate;
        IsSecret = fileInfo.IsSecret;
        Status = fileInfo.Status;
    }

    private string GetFileIcon()
    {
        var ext = Path.GetExtension(FileName).ToLowerInvariant();

        return ext switch
        {
            ".sh" or ".bash" or ".zsh" or ".fish" => "Terminal",
            ".vim" or ".vimrc" or ".nvim" => "Code",
            ".git" or ".gitconfig" or ".gitignore" => "Git",
            ".json" or ".yaml" or ".yml" or ".toml" => "Settings",
            ".conf" or ".config" or ".cfg" => "Config",
            ".ssh" => "Key",
            _ when FileName.StartsWith(".") => "DotFile",
            _ => "File"
        };
    }
}
