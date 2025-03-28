using System;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace TelltaleTextureTool.Utilities;

/// <summary>
/// A helper class for the dialog messages. 
/// </summary>
public static class MessageBoxes
{
    public const string ASSETS_DIRECTORY = "avares://TelltaleTextureTool/Assets/";
    public const string APP_ICON = ASSETS_DIRECTORY + "main_icon.ico";

    /// <summary>
    /// Creates and returns the error dialog.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static MessageBoxStandardParams GetSuccessBox(string message)
    {
        var bitmap = new Bitmap(AssetLoader.Open(new Uri(APP_ICON)));

        return new MessageBoxStandardParams
        {
            ButtonDefinitions = ButtonEnum.Ok,
            ContentTitle = "Success",
            ContentMessage = message,
            Icon = Icon.Success,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            WindowIcon = new WindowIcon(bitmap),
            CanResize = false,
            MaxWidth = 500,
            MaxHeight = 800,
            SizeToContent = SizeToContent.WidthAndHeight,
            ShowInCenter = true,
            Topmost = false,
            EnterDefaultButton = ClickEnum.Ok,
            EscDefaultButton = ClickEnum.Ok
        };
    }

    /// <summary>
    /// Creates and returns the error dialog.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static MessageBoxStandardParams GetErrorBox(string message)
    {
        var bitmap = new Bitmap(AssetLoader.Open(new Uri(APP_ICON)));

        return new MessageBoxStandardParams
        {
            ButtonDefinitions = ButtonEnum.Ok,
            ContentTitle = "Error",
            ContentMessage = message,
            Icon = Icon.Error,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            WindowIcon = new WindowIcon(bitmap),
            CanResize = false,
            MaxWidth = 500,
            MaxHeight = 800,
            SizeToContent = SizeToContent.WidthAndHeight,
            ShowInCenter = true,
            Topmost = false,
            EnterDefaultButton = ClickEnum.Ok,
            EscDefaultButton = ClickEnum.Ok
        };
    }

    /// <summary>
    /// Creates and returns the confirmation dialog.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static MessageBoxStandardParams GetConfirmationBox(string message)
    {
        var bitmap = new Bitmap(AssetLoader.Open(new Uri(APP_ICON)));

        return new MessageBoxStandardParams
        {
            ButtonDefinitions = ButtonEnum.YesNoCancel,
            EnterDefaultButton = ClickEnum.Cancel,
            EscDefaultButton = ClickEnum.Cancel,
            ContentTitle = "Confirm",
            ContentMessage = message,
            Icon = Icon.Question,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            WindowIcon = new WindowIcon(bitmap),
            CanResize = false,
            MaxWidth = 500,
            MaxHeight = 800,
            SizeToContent = SizeToContent.WidthAndHeight,
            ShowInCenter = true,
            Topmost = false,
        };
    }
}
