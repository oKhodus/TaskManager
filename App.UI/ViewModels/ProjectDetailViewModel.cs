using System;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.UI.ViewModels;

/// <summary>
/// ViewModel for project details
/// Validation performed manually for better UX
/// </summary>
public partial class ProjectDetailViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectDetailViewModel> _logger;

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

    public ProjectDetailViewModel(
        IProjectService projectService,
        ILogger<ProjectDetailViewModel> logger)
    {
        _projectService = projectService;
        _logger = logger;
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
        _logger.LogDebug("SaveAsync called for project: Name={Name}, Key={Key}, IsCreateMode={IsCreateMode}", Name, Key, IsCreateMode);

        // Clear all errors
        NameError = null;
        KeyError = null;

        // Validate fields
        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Name is required";
            _logger.LogDebug("Validation failed: Name is empty");
        }

        if (string.IsNullOrWhiteSpace(Key))
        {
            KeyError = "Key is required";
            _logger.LogDebug("Validation failed: Key is empty");
        }

        // If validation failed, return early
        if (NameError != null || KeyError != null)
        {
            return;
        }

        // Check Key uniqueness (exclude current project ID in edit mode)
        var excludeId = IsCreateMode ? null : (Guid?)Id;
        var isKeyUnique = await _projectService.IsKeyUniqueAsync(Key, excludeId);

        if (!isKeyUnique)
        {
            KeyError = "A project with this key already exists";
            _logger.LogInformation("Key uniqueness validation failed: Key={Key} already exists", Key);
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
            try
            {
                if (IsCreateMode)
                {
                    _logger.LogInformation("Creating new project: Name={Name}, Key={Key}", Name, Key);

                    // Create new project
                    var createdProject = await _projectService.CreateProjectAsync(project);

                    // Update Id with generated value
                    Id = createdProject.Id;
                    CreatedAt = createdProject.CreatedAt;

                    _logger.LogInformation("Project created successfully: Id={ProjectId}", Id);

                    // Notify that project was created
                    ProjectSaved?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _logger.LogInformation("Updating project: Id={ProjectId}, Name={Name}, Key={Key}", Id, Name, Key);

                    // Update existing project
                    await _projectService.UpdateProjectAsync(project);

                    _logger.LogInformation("Project updated successfully: Id={ProjectId}", Id);
                }

                IsEditMode = false;
                IsCreateMode = false;
            }
            catch (DbUpdateException ex)
            {
                // Handle database constraint violations (e.g., UNIQUE constraint on Key)
                _logger.LogError(ex, "Database error while saving project: Name={Name}, Key={Key}", Name, Key);

                // Check if this is a UNIQUE constraint violation on Key
                if (ex.InnerException?.Message?.Contains("UNIQUE constraint failed: Projects.Key") == true)
                {
                    KeyError = "A project with this key already exists";
                    _logger.LogWarning("UNIQUE constraint violation detected for Key={Key}", Key);
                }
                else
                {
                    // Generic error for other database issues
                    NameError = "An error occurred while saving the project";
                    _logger.LogError("Unexpected database error: {Message}", ex.Message);
                }
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                _logger.LogError(ex, "Unexpected error while saving project: Name={Name}, Key={Key}", Name, Key);
                NameError = "An unexpected error occurred";
            }
        }
        else
        {
            // If service validation fails, show error on Name field
            NameError = "Project validation failed";
            _logger.LogWarning("Project validation failed: Name={Name}, Key={Key}", Name, Key);
        }
    }
}
