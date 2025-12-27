using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using App.UI.ViewModels;
using App.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace App.UI.Views;

public partial class KanbanBoardView : UserControl
{
    private KanbanCardViewModel? _draggedCard;
    private TaskStatus? _draggedFromStatus;

    public KanbanBoardView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Attach pointer pressed handlers to all card borders for drag initiation
        AttachDragHandlers();

        // Subscribe to DataContext changes to reload
        DataContextChanged += OnDataContextChanged;
    }

    private async void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is KanbanBoardViewModel viewModel)
        {
            // Unsubscribe from previous viewModel if any
            viewModel.TaskDetailRequested -= OnTaskDetailRequested;

            // Subscribe to task detail request event
            viewModel.TaskDetailRequested += OnTaskDetailRequested;

            // Load projects when view is ready
            await viewModel.LoadProjectsCommand.ExecuteAsync(null);
        }
    }

    private void OnTaskDetailRequested(object? sender, Guid taskId)
    {
        // Get TaskDetailWindowViewModel from DI
        var serviceProvider = App.ServiceProvider;
        if (serviceProvider == null) return;

        var detailViewModel = serviceProvider.GetRequiredService<TaskDetailWindowViewModel>();

        // Subscribe to TaskUpdated event to refresh board
        detailViewModel.TaskUpdated += async (s, e) =>
        {
            if (DataContext is KanbanBoardViewModel boardViewModel)
            {
                await boardViewModel.RefreshBoardAsync();
            }
        };

        // Get parent window
        var parentWindow = this.VisualRoot as Window;
        if (parentWindow == null) return;

        // Show task detail window
        TaskDetailWindow.ShowForTask(taskId, detailViewModel, parentWindow);
    }

    private void AttachDragHandlers()
    {
        // Find all card borders in the visual tree and attach handlers
        var itemsControls = this.GetVisualDescendants().OfType<ItemsControl>();

        foreach (var itemsControl in itemsControls)
        {
            if (itemsControl.Name == "CardsItemsControl")
            {
                itemsControl.LayoutUpdated += (s, e) =>
                {
                    var borders = itemsControl.GetVisualDescendants().OfType<Border>()
                        .Where(b => b.Name == "CardBorder");

                    foreach (var border in borders)
                    {
                        border.PointerPressed -= OnCardPointerPressed;
                        border.PointerPressed += OnCardPointerPressed;
                    }
                };
            }
        }
    }

    private void OnCardDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is KanbanCardViewModel card)
        {
            // Trigger task detail window via ViewModel
            if (DataContext is KanbanBoardViewModel viewModel)
            {
                viewModel.OpenTaskDetailCommand.Execute(card.Id);
            }

            // Mark event as handled to prevent drag from starting
            e.Handled = true;
        }
    }

    private async void OnCardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is KanbanCardViewModel card)
        {
            _draggedCard = card;
            _draggedFromStatus = card.Status;

            #pragma warning disable CS0618 // Type or member is obsolete
            var dragData = new DataObject();
            dragData.Set("KanbanCard", card);

            await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
            #pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        // Only allow drop if we're dragging a card
        #pragma warning disable CS0618 // Type or member is obsolete
        if (e.Data.Contains("KanbanCard"))
        #pragma warning restore CS0618 // Type or member is obsolete
        {
            e.DragEffects = DragDropEffects.Move;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private async void Drop(object? sender, DragEventArgs e)
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        if (!e.Data.Contains("KanbanCard") || _draggedCard == null)
        #pragma warning restore CS0618 // Type or member is obsolete
        {
            return;
        }

        // Find which column the card was dropped on
        // Try to get the drop target from the event source
        Control? dropTarget = null;

        // First try e.Source
        if (e.Source is Control sourceControl)
        {
            dropTarget = sourceControl;
        }

        // If that doesn't work, try getting the visual element at the pointer position
        if (dropTarget == null)
        {
            return;
        }

        // Walk up the visual tree to find the column border
        var column = FindParentColumn(dropTarget);
        if (column?.DataContext is KanbanColumnViewModel columnViewModel)
        {
            var newStatus = columnViewModel.Status;

            // Update via ViewModel
            if (DataContext is KanbanBoardViewModel boardViewModel)
            {
                await boardViewModel.OnCardDroppedAsync(_draggedCard, newStatus);
            }
        }

        _draggedCard = null;
        _draggedFromStatus = null;
    }

    private Control? FindParentColumn(Control control)
    {
        // Start from the control itself, then walk up
        var current = control as Visual;

        while (current != null)
        {
            // Check if this is a column border (has Name="ColumnBorder")
            if (current is Border border && border.Name == "ColumnBorder")
            {
                return border;
            }

            // Also check by DataContext for backwards compatibility
            if (current is Border borderWithContext &&
                borderWithContext.DataContext is KanbanColumnViewModel)
            {
                return borderWithContext;
            }

            current = current.GetVisualParent();
        }

        return null;
    }
}
