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
            dashboardViewModel.ProjectMasterViewModel.CreateUserRequested += OnCreateUserRequested;
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
}
