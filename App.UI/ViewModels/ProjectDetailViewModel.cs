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
    private bool _isCreateMode;

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

    public event EventHandler? ProjectSaved;

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
        IsCreateMode = false;
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

        // Raise event to hide container if in create mode
        if (IsCreateMode)
        {
            IsCreateMode = false;
            ProjectCancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? ProjectCancelled;

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
            if (IsCreateMode)
            {
                // Create new project
                var createdProject = await _projectService.CreateProjectAsync(project);

                // Update Id with generated value
                Id = createdProject.Id;
                CreatedAt = createdProject.CreatedAt;

                // Notify that project was created
                ProjectSaved?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Update existing project
                await _projectService.UpdateProjectAsync(project);
            }

            IsEditMode = false;
            IsCreateMode = false;
        }
        else
        {
            // If service validation fails, show error on Name field
            NameError = "Project validation failed";
        }
    }
}
