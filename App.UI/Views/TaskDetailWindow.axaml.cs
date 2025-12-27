using System;
using Avalonia.Controls;
using App.UI.ViewModels;

namespace App.UI.Views;

public partial class TaskDetailWindow : Window
{
    public TaskDetailWindow()
    {
        InitializeComponent();
    }

    public TaskDetailWindow(TaskDetailWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Subscribe to close event
        viewModel.CloseRequested += (s, e) => Close();
    }

    /// <summary>
    /// Factory method to create and show window with task details
    /// </summary>
    public static async void ShowForTask(Guid taskId, TaskDetailWindowViewModel viewModel, Window owner)
    {
        var window = new TaskDetailWindow(viewModel);

        // Load task data before showing
        await viewModel.LoadTaskAsync(taskId);

        // Show as dialog
        await window.ShowDialog(owner);
    }
}
