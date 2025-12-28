using System;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.UI.ViewModels;

/// <summary>
/// ViewModel for project details
/// Validation performed manually for better UX
/// </summary>
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
    private string _key = string.Empty;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime? _updatedAt;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string? _nameError;

    [ObservableProperty]
    private string? _keyError;

    partial void OnNameChanged(string value)
    {
        NameError = null;
    }

    partial void OnKeyChanged(string value)
    {
        KeyError = null;
    }

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
        // Clear all errors
        NameError = null;
        KeyError = null;

        // Validate fields
        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Name is required";
        }

        if (string.IsNullOrWhiteSpace(Key))
        {
            KeyError = "Key is required";
        }

        // If validation failed, return early
        if (NameError != null || KeyError != null)
        {
            return;
        }

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
        else
        {
            // If service validation fails, show error on Name field
            NameError = "Project validation failed";
        }
    }
}
