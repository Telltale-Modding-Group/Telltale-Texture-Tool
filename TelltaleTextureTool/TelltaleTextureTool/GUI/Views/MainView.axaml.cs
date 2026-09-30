using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using TelltaleTextureTool.ViewModels;

namespace TelltaleTextureTool.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        if (DataContext is MainViewModel mainVm)
        {
            // mainVm.
            // ConfigHelper.Save(
            //     new AppConfig
            //     {
            //         LastFolder = mainVm.FileExplorerContext.CurrentDirectory,
            //         //  Theme = desktop.MainWindow.DataContext is MainViewModel vm2 ? vm2.Theme : "Light"
            //     }
            // );
        }

        // DataContext.Exit += (_, _) =>
        // {
        //     // Save configuration on shutdown
        //     try
        //     {

        //     catch (Exception ex)
        //     {
        //         Console.Error.WriteLine("Error saving configuration: " + ex.Message);
        //     }
        // };
    }

    private void ResetPanAndZoom()
    {
        // Assuming zoomBorder is the name of your ZoomBorder control
        ZoomBorder1.ResetMatrix();
    }

    private void ScrollIntoView()
    {
        // Dispatcher.UIThread.Post(
        //     () =>
        //     {
        //         TextureDirectoryFilesDataGrid.ScrollIntoView(
        //             viewModel.FileExplorerContext.SelectedItem,
        //             null
        //         );
        //     },
        //     DispatcherPriority.Background
        // );
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

    private void Binding(
        object? sender,
        Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e
    ) { }

    private void PreviewImageCommand(object? sender, Avalonia.Interactivity.RoutedEventArgs e) { }

    public void FindSelectedItem()
    {
        if (DataContext is MainViewModel viewModel)
        {
            // Scroll to the selected item
            Dispatcher.UIThread.Post(
                () =>
                {
                    TextureDirectoryFilesDataGrid.ScrollIntoView(
                        viewModel.FileExplorerContext.SelectedItem,
                        null
                    );
                },
                DispatcherPriority.Background
            );
        }
    }

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

            viewModel.FileExplorerContext.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(viewModel.FileExplorerContext.SelectedItem))
                {
                    // Scroll to the selected item
                    Dispatcher.UIThread.Post(
                        () =>
                        {
                            TextureDirectoryFilesDataGrid.ScrollIntoView(
                                viewModel.FileExplorerContext.SelectedItem,
                                null
                            );
                        },
                        DispatcherPriority.Background
                    );
                }
            };
        }

        ResetPanAndZoom();
    }
}
