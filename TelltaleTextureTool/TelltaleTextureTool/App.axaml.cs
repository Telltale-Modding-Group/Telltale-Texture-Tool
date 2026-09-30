using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TelltaleTextureTool.ViewModels;
using TelltaleTextureTool.Views;

namespace TelltaleTextureTool;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new ServiceCollection();

                desktop.MainWindow = new MainWindow { DataContext = new MainViewModel() };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView { DataContext = new MainViewModel() };
            }

            //             services.AddSingleton<AppSettings>(_ => AppSettings.Load());
            // services.AddSingleton<MainModel>();
            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception e)
        {
            Logger.Log(e);
        }
    }
}
