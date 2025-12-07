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
    private readonly ProjectDetailViewModel _projectDetailViewModel;

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private bool _isLoading;

    public ProjectDetailViewModel ProjectDetailViewModel => _projectDetailViewModel;

    public ProjectMasterViewModel(
        IProjectService projectService,
        IExportService exportService,
        ProjectDetailViewModel projectDetailViewModel)
    {
        _projectService = projectService;
        _exportService = exportService;
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

        var fileName = $"Projects_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

        await _exportService.ExportToCsvAsync(Projects, filePath);
    }
}
