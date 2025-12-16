using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.UI.ViewModels;

public partial class ProjectMasterViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;
    private readonly IExportService _exportService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ProjectDetailViewModel _projectDetailViewModel;

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
        ProjectDetailViewModel projectDetailViewModel)
    {
        _projectService = projectService;
        _exportService = exportService;
        _currentUserService = currentUserService;
        _projectDetailViewModel = projectDetailViewModel;
    }

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        IsLoading = true;
        try
        {
            var projects = await _projectService.GetActiveProjectsAsync();
            Projects = new ObservableCollection<Project>(projects);
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
            return;

        try
        {
            var fileName = $"Projects_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            await _exportService.ExportToCsvAsync(Projects, filePath);

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
            // Log error - TODO: Show notification to user
            Console.WriteLine($"Export failed: {ex.Message}");
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
