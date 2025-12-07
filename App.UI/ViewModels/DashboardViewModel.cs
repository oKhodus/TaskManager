using CommunityToolkit.Mvvm.ComponentModel;

namespace App.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    [ObservableProperty]
    private ProjectMasterViewModel? _projectMasterViewModel;

    [ObservableProperty]
    private TaskMasterViewModel? _taskMasterViewModel;

    [ObservableProperty]
    private int _selectedTabIndex;

    public DashboardViewModel(
        ProjectMasterViewModel projectMasterViewModel,
        TaskMasterViewModel taskMasterViewModel)
    {
        _projectMasterViewModel = projectMasterViewModel;
        _taskMasterViewModel = taskMasterViewModel;
        _selectedTabIndex = 0;
    }
}
