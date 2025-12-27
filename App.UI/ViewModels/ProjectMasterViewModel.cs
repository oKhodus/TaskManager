using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace App.UI.ViewModels;

public partial class ProjectMasterViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;
    private readonly IExportService _exportService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ProjectDetailViewModel _projectDetailViewModel;
    private readonly ILogger<ProjectMasterViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Check if current user is admin (for UI visibility)
    /// </summary>
    public bool IsAdmin => _currentUserService.IsAdmin;

    public ProjectDetailViewModel ProjectDetailViewModel => _projectDetailViewModel;

    public ProjectMasterViewModel(
        IProjectService projectService,
        IExportService exportService,
        ICurrentUserService currentUserService,
        ProjectDetailViewModel projectDetailViewModel,
        ILogger<ProjectMasterViewModel> logger)
    {
        _projectService = projectService;
        _exportService = exportService;
        _currentUserService = currentUserService;
        _projectDetailViewModel = projectDetailViewModel;
        _logger = logger;

        _logger.LogDebug("ProjectMasterViewModel initialized");
    }

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        _logger.LogInformation("Loading projects...");
        IsLoading = true;
        try
        {
            var projects = await _projectService.GetActiveProjectsAsync();
            Projects = new ObservableCollection<Project>(projects);
            _logger.LogInformation("Loaded {ProjectCount} projects", projects.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load projects");
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedProjectChanged(Project? value)
    {
        if (value != null)
        {
            _projectDetailViewModel.LoadProject(value);
        }
    }

    [RelayCommand]
    private async Task ExportToCsvAsync()
    {
        if (Projects.Count == 0)
        {
            _logger.LogWarning("Export attempted with no projects available");
            return;
        }

        try
        {
            var fileName = $"Projects_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            _logger.LogInformation("Exporting {ProjectCount} projects to {FilePath}", Projects.Count, filePath);
            await _exportService.ExportToCsvAsync(Projects, filePath);
            _logger.LogInformation("Export completed successfully");

            // Open folder with exported file for better UX
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = Path.GetDirectoryName(filePath),
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export to CSV failed");
            // TODO: Show notification to user
        }
    }

    [RelayCommand]
    private void OpenCreateUserDialog()
    {
        // This will be handled in the View code-behind
        // We raise an event that the View can subscribe to
        CreateUserRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? CreateUserRequested;
}
