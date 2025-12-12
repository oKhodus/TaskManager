using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.UI.ViewModels;

public partial class ProjectDetailViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    [MaxLength(10, ErrorMessage = "Project key must not exceed 10 characters")]
    private string _key = string.Empty;

    [ObservableProperty]
    private bool _hasProjectLoaded;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime? _updatedAt;

    [ObservableProperty]
    private bool _isEditMode;

    public ProjectDetailViewModel(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public void LoadProject(Project project)
    {
        Id = project.Id;
        Name = project.Name;
        Description = project.Description;
        Key = project.Key ?? string.Empty;
        IsActive = project.IsActive;
        CreatedAt = project.CreatedAt;
        UpdatedAt = project.UpdatedAt;
        IsEditMode = false;
        HasProjectLoaded = true;
    }

    public void ClearProject()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        Description = null;
        Key = string.Empty;
        IsActive = true;
        CreatedAt = DateTime.MinValue;
        UpdatedAt = null;
        IsEditMode = false;
        HasProjectLoaded = false;
    }

    [RelayCommand]
    private void StartEdit()
    {
        IsEditMode = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var project = new Project
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Key = Key,
            IsActive = IsActive,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };

        if (await _projectService.ValidateProjectAsync(project))
        {
            await _projectService.UpdateProjectAsync(project);
            IsEditMode = false;
        }
    }
}
