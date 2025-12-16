using Avalonia.Controls;
using App.UI.ViewModels;

namespace App.UI.Views;

public partial class CreateTaskWindow : Window
{
    public CreateTaskWindow()
    {
        InitializeComponent();
    }

    public CreateTaskWindow(CreateTaskViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Load data when window opens
        Loaded += async (s, e) => await viewModel.LoadDataCommand.ExecuteAsync(null);

        // Close window after task is created
        viewModel.TaskCreated += (s, e) =>
        {
            // Close after a short delay to show success message
            System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => Close());
            });
        };
    }
}
