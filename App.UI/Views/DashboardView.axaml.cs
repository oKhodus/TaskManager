using Avalonia.Controls;
using App.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace App.UI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();

        // Subscribe to DataContextChanged to wire up event handlers
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is DashboardViewModel dashboardViewModel)
        {
            // Subscribe to CreateUserRequested event
            if (dashboardViewModel.ProjectMasterViewModel != null)
            {
                dashboardViewModel.ProjectMasterViewModel.CreateUserRequested += OnCreateUserRequested;
            }

            // Subscribe to CreateTaskRequested event from Kanban board
            if (dashboardViewModel.KanbanBoardViewModel != null)
            {
                dashboardViewModel.KanbanBoardViewModel.CreateTaskRequested += OnCreateTaskRequested;
            }
        }
    }

    private async void OnCreateUserRequested(object? sender, System.EventArgs e)
    {
        // Get CreateUserViewModel from DI
        var createUserViewModel = App.ServiceProvider?.GetRequiredService<CreateUserViewModel>();

        if (createUserViewModel != null)
        {
            var window = new CreateUserWindow(createUserViewModel);

            // Get parent window
            var parentWindow = this.VisualRoot as Window;
            if (parentWindow != null)
            {
                await window.ShowDialog(parentWindow);

                // Optionally reload projects after user creation
                // if (DataContext is DashboardViewModel vm)
                // {
                //     await vm.ProjectMasterViewModel.LoadProjectsCommand.ExecuteAsync(null);
                // }
            }
        }
    }

    private async void OnCreateTaskRequested(object? sender, System.EventArgs e)
    {
        // Get CreateTaskViewModel from DI
        var createTaskViewModel = App.ServiceProvider?.GetRequiredService<CreateTaskViewModel>();

        if (createTaskViewModel != null && DataContext is DashboardViewModel dashboardViewModel)
        {
            // If a project is selected in Kanban, pre-fill it
            var selectedProject = dashboardViewModel.KanbanBoardViewModel?.SelectedProject;
            if (selectedProject != null)
            {
                await createTaskViewModel.LoadDataCommand.ExecuteAsync(null);
                createTaskViewModel.SetProject(selectedProject);
            }

            var window = new CreateTaskWindow(createTaskViewModel);

            // Get parent window
            var parentWindow = this.VisualRoot as Window;
            if (parentWindow != null)
            {
                await window.ShowDialog(parentWindow);

                // Reload Kanban board after task creation
                if (dashboardViewModel.KanbanBoardViewModel != null)
                {
                    await dashboardViewModel.KanbanBoardViewModel.RefreshBoardAsync();
                }
            }
        }
    }
}
