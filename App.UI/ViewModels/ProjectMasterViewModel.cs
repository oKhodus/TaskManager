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

    [ObservableProperty]
    private bool _isProjectDetailVisible;

    [ObservableProperty]
    private bool _isCreateMode;

    [ObservableProperty]
    private bool _isProjectsListVisible;

    [ObservableProperty]
    private bool _isDeleteConfirmationVisible;

    [ObservableProperty]
    private string _projectToDeleteName = string.Empty;

    private Project? _projectPendingDeletion;

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

        // Subscribe to ProjectDetailViewModel events
        _projectDetailViewModel.ProjectSaved += OnProjectSaved;
        _projectDetailViewModel.ProjectCancelled += OnProjectCancelled;

        _logger.LogDebug("ProjectMasterViewModel initialized");
    }

    private async void OnProjectSaved(object? sender, EventArgs e)
    {
        _logger.LogInformation("Project saved - refreshing project list");
        await LoadProjectsAsync();

        // After loading, select the newly created project
        if (_projectDetailViewModel.Id != Guid.Empty)
        {
            var createdProject = Projects.FirstOrDefault(p => p.Id == _projectDetailViewModel.Id);
            if (createdProject != null)
            {
                SelectedProject = createdProject;
                _logger.LogDebug("Selected newly created project: Id={ProjectId}", createdProject.Id);
            }
        }
    }

    private void OnProjectCancelled(object? sender, EventArgs e)
    {
        _logger.LogInformation("Project creation/edit cancelled - hiding detail container");
        IsProjectDetailVisible = false;
        IsCreateMode = false;
        SelectedProject = null;
    }

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        // Toggle behavior: if list is already visible, hide it
        if (IsProjectsListVisible && Projects.Count > 0)
        {
            _logger.LogInformation("Toggling projects list visibility - hiding list");
            IsProjectsListVisible = false;
            IsProjectDetailVisible = false;
            IsCreateMode = false;
            SelectedProject = null;
            return;
        }

        _logger.LogInformation("Loading projects...");
        IsLoading = true;
        try
        {
            var projects = await _projectService.GetActiveProjectsAsync();
            Projects = new ObservableCollection<Project>(projects);
            _logger.LogInformation("Loaded {ProjectCount} projects", projects.Count());

            // Show projects list and hide detail container
            IsProjectsListVisible = true;
            IsProjectDetailVisible = false;
            IsCreateMode = false;
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
            _logger.LogDebug("Project selected: Id={ProjectId}, Name={Name}", value.Id, value.Name);
            _projectDetailViewModel.LoadProject(value);
            _projectDetailViewModel.IsCreateMode = false;
            IsProjectDetailVisible = true;
            IsCreateMode = false;
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

    [RelayCommand]
    private void CreateNewProject()
    {
        _logger.LogInformation("Creating new project - opening form");

        // Clear ProjectDetailViewModel
        _projectDetailViewModel.LoadProject(new Project
        {
            Id = Guid.Empty,
            Name = string.Empty,
            Key = string.Empty,
            Description = string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Set create mode - fields should be editable
        _projectDetailViewModel.IsCreateMode = true;
        _projectDetailViewModel.IsEditMode = true; // Make fields editable
        IsProjectDetailVisible = true;
        IsCreateMode = true;

        // Deselect any selected project
        SelectedProject = null;

        _logger.LogDebug("New project form opened in create mode");
    }

    [RelayCommand]
    private void DeleteProject()
    {
        if (SelectedProject == null)
        {
            _logger.LogWarning("Delete attempted with no project selected");
            return;
        }

        // Store project for deletion and show confirmation dialog
        _projectPendingDeletion = SelectedProject;
        ProjectToDeleteName = SelectedProject.Name;
        IsDeleteConfirmationVisible = true;
        _logger.LogInformation("Delete confirmation requested for project: Id={ProjectId}, Name={Name}",
            SelectedProject.Id, SelectedProject.Name);
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        if (_projectPendingDeletion == null)
        {
            _logger.LogWarning("Confirm delete called but no project pending deletion");
            IsDeleteConfirmationVisible = false;
            return;
        }

        var projectToDelete = _projectPendingDeletion;
        _logger.LogInformation("User confirmed deletion for project: Id={ProjectId}, Name={Name}",
            projectToDelete.Id, projectToDelete.Name);

        try
        {
            // Hide confirmation dialog
            IsDeleteConfirmationVisible = false;

            // Call service to soft delete
            await _projectService.DeleteProjectAsync(projectToDelete.Id);
            _logger.LogInformation("Project deleted successfully: Id={ProjectId}", projectToDelete.Id);

            // Remove from collection
            Projects.Remove(projectToDelete);

            // Hide detail container
            IsProjectDetailVisible = false;
            IsCreateMode = false;
            SelectedProject = null;

            _logger.LogDebug("Project removed from UI collection");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete project: Id={ProjectId}", projectToDelete.Id);
            // TODO: Show error notification to user
            throw;
        }
        finally
        {
            _projectPendingDeletion = null;
            ProjectToDeleteName = string.Empty;
        }
    }

    [RelayCommand]
    private void CancelDelete()
    {
        _logger.LogInformation("User cancelled deletion for project: Name={Name}", ProjectToDeleteName);
        IsDeleteConfirmationVisible = false;
        _projectPendingDeletion = null;
        ProjectToDeleteName = string.Empty;
    }

    public event EventHandler? CreateUserRequested;
}
