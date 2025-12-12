using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskStatus = App.Domain.Enums.TaskStatus;

namespace App.UI.ViewModels;

public partial class TaskMasterViewModel : ViewModelBase
{
    private readonly ITaskService _taskService;
    private readonly IExportService _exportService;
    private readonly TaskDetailViewModel _taskDetailViewModel;

    [ObservableProperty]
    private ObservableCollection<TaskBase> _tasks = new();

    [ObservableProperty]
    private TaskBase? _selectedTask;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private TaskStatus? _selectedStatus;

    [ObservableProperty]
    private TaskPriority? _selectedPriority;

    public ObservableCollection<TaskStatus?> AvailableStatuses { get; } = new()
    {
        null,
        TaskStatus.ToDo,
        TaskStatus.InProgress,
        TaskStatus.InReview,
        TaskStatus.Done,
        TaskStatus.Blocked,
        TaskStatus.Cancelled
    };

    public ObservableCollection<TaskPriority?> AvailablePriorities { get; } = new()
    {
        null,
        TaskPriority.Low,
        TaskPriority.Medium,
        TaskPriority.High,
        TaskPriority.Critical
    };

    public TaskDetailViewModel TaskDetailViewModel => _taskDetailViewModel;

    public TaskMasterViewModel(
        ITaskService taskService,
        IExportService exportService,
        TaskDetailViewModel taskDetailViewModel)
    {
        _taskService = taskService;
        _exportService = exportService;
        _taskDetailViewModel = taskDetailViewModel;
    }

    [RelayCommand]
    private async Task LoadTasksAsync()
    {
        IsLoading = true;
        try
        {
            var tasks = await _taskService.GetAllTasksAsync();
            Tasks = new ObservableCollection<TaskBase>(tasks);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsLoading = true;
        try
        {
            IEnumerable<TaskBase> tasks;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                tasks = await _taskService.SearchTasksAsync(SearchText);
            }
            else if (SelectedStatus.HasValue)
            {
                tasks = await _taskService.GetTasksByStatusAsync(SelectedStatus.Value);
            }
            else if (SelectedPriority.HasValue)
            {
                tasks = await _taskService.GetTasksByPriorityAsync(SelectedPriority.Value);
            }
            else
            {
                tasks = await _taskService.GetAllTasksAsync();
            }

            Tasks = new ObservableCollection<TaskBase>(tasks);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedStatus = null;
        SelectedPriority = null;
        await LoadTasksAsync();
    }

    partial void OnSelectedTaskChanged(TaskBase? value)
    {
        if (value != null)
        {
            _taskDetailViewModel.LoadTask(value);
        }
        else
        {
            _taskDetailViewModel.ClearTask();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) && Tasks.Count > 0)
        {
            // Auto-search when text is cleared
            SearchCommand.Execute(null);
        }
    }

    partial void OnSelectedStatusChanged(TaskStatus? value)
    {
        SearchCommand.Execute(null);
    }

    partial void OnSelectedPriorityChanged(TaskPriority? value)
    {
        SearchCommand.Execute(null);
    }

    [RelayCommand]
    private async Task ExportToCsvAsync()
    {
        if (Tasks.Count == 0)
            return;

        var fileName = $"Tasks_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

        await _exportService.ExportToCsvAsync(Tasks, filePath);
    }
}
