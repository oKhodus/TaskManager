using System.Collections.ObjectModel;
using App.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.UI.ViewModels;

/// <summary>
/// ViewModel for a single Kanban column (status group)
/// Follows SRP - manages only one column's tasks and drag-drop logic
/// </summary>
public partial class KanbanColumnViewModel : ViewModelBase
{
    [ObservableProperty]
    private TaskStatus _status;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private ObservableCollection<KanbanCardViewModel> _cards = new();

    /// <summary>
    /// Number of tasks in this column
    /// </summary>
    public int TaskCount => Cards.Count;

    /// <summary>
    /// Column header with count
    /// </summary>
    public string Header => $"{Title} ({TaskCount})";

    public KanbanColumnViewModel(TaskStatus status)
    {
        Status = status;
        Title = GetStatusDisplayName(status);
    }

    private static string GetStatusDisplayName(TaskStatus status) => status switch
    {
        TaskStatus.Todo => "Todo",
        TaskStatus.Assigned => "Assigned",
        TaskStatus.InProgress => "In Progress",
        TaskStatus.Review => "Review",
        TaskStatus.Done => "Done",
        _ => status.ToString()
    };

    public void AddCard(KanbanCardViewModel card)
    {
        Cards.Add(card);
        OnPropertyChanged(nameof(TaskCount));
        OnPropertyChanged(nameof(Header));
    }

    public void RemoveCard(KanbanCardViewModel card)
    {
        Cards.Remove(card);
        OnPropertyChanged(nameof(TaskCount));
        OnPropertyChanged(nameof(Header));
    }

    public void ClearCards()
    {
        Cards.Clear();
        OnPropertyChanged(nameof(TaskCount));
        OnPropertyChanged(nameof(Header));
    }
}
