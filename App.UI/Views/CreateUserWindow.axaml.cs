using Avalonia.Controls;
using App.UI.ViewModels;

namespace App.UI.Views;

public partial class CreateUserWindow : Window
{
    public CreateUserWindow()
    {
        InitializeComponent();
    }

    public CreateUserWindow(CreateUserViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Close window when user is created
        viewModel.UserCreated += (s, e) =>
        {
            // Keep window open briefly to show success message
            System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => Close());
            });
        };
    }
}
