using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using TelltaleTextureTool.GUI.ViewModels;
using TelltaleTextureTool.ViewModels;

namespace TelltaleTextureTool.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        DataContext = new MainViewModel();
    }

    private void ResetPanAndZoom()
    {
        // Assuming zoomBorder is the name of your ZoomBorder control
        ZoomBorder1.ResetMatrix();
    }

    private void ZoomBorder_KeyDown(object? sender, KeyEventArgs e)
    {
        var zoomBorder = this.DataContext as ZoomBorder;

        switch (e.Key)
        {
            case Key.F:
                zoomBorder?.Fill();
                break;
            case Key.U:
                zoomBorder?.Uniform();
                break;
            case Key.R:
                zoomBorder?.ResetMatrix();
                break;
            case Key.T:
                zoomBorder?.ToggleStretchMode();
                zoomBorder?.AutoFit();
                break;
        }
    }

    private void OnDataContextChanged(object sender, EventArgs e)
    {
        if (DataContext is MainViewModel viewModel) { }
    }

    private void Binding_1(object? sender, Avalonia.Controls.SelectionChangedEventArgs e) { }

    private void PreviewImageCommand_1(
        object? sender,
        Avalonia.Controls.SelectionChangedEventArgs e
    ) { }

    private void Binding(
        object? sender,
        Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e
    ) { }

    private void PreviewImageCommand_1(
        object? sender,
        Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e
    ) { }

    private void PreviewImageCommand(object? sender, Avalonia.Interactivity.RoutedEventArgs e) { }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.ResetPanAndZoomCommand = new RelayCommand(ResetPanAndZoom);

            viewModel.NotificationManager = new WindowNotificationManager(
                TopLevel.GetTopLevel(this)!
            )
            {
                MaxItems = 5,
                Position = NotificationPosition.BottomRight,
            };

            viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(viewModel.DataGridSelectedItem))
                {
                    // Scroll to the selected item
                    Dispatcher.UIThread.Post(
                        () =>
                        {
                            if (viewModel.DataGridSelectedItem != null)
                            {
                                TextureDirectoryFilesDataGrid.ScrollIntoView(
                                    viewModel.DataGridSelectedItem,
                                    null
                                );
                            }
                        },
                        DispatcherPriority.Background
                    );
                }
            };
        }
        ResetPanAndZoom();
    }
}
