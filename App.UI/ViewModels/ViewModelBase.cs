using CommunityToolkit.Mvvm.ComponentModel;

namespace App.UI.ViewModels;

/// <summary>
/// Base ViewModel - uses ObservableObject instead of ObservableValidator
/// Validation is performed manually in submit methods for better UX
/// (no validation errors on initialization, only after user interaction)
/// </summary>
public class ViewModelBase : ObservableObject
{
}
