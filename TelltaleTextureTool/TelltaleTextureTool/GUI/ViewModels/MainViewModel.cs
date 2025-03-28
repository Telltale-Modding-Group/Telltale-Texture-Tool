using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Svg.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using TelltaleTextureTool.Codecs;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.Utilities;
using TelltaleTextureTool.Views;
using IImage = Avalonia.Media.IImage;
using Texture = TelltaleTextureTool.Graphics.Texture;

namespace TelltaleTextureTool.ViewModels;

public class EnumDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return string.Empty;

        // Get the field in the enum type that matches the current enum value
        FieldInfo field = value.GetType().GetField(value.ToString());

        // Get the Display attribute if present
        DisplayAttribute attribute = field
            ?.GetCustomAttributes(false)
            .OfType<DisplayAttribute>()
            .FirstOrDefault();

        // Return the name if available, otherwise fall back to the enum value's name
        return attribute?.Name ?? value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Reverse the conversion if needed
        if (value is string stringValue)
        {
            foreach (var field in targetType.GetFields())
            {
                var attribute = field
                    .GetCustomAttributes(false)
                    .OfType<DisplayAttribute>()
                    .FirstOrDefault();

                if (attribute?.Name == stringValue || field.Name == stringValue)
                {
                    return Enum.Parse(targetType, field.Name);
                }
            }
        }

        throw new InvalidOperationException("Cannot convert back.");
    }
}

public partial class MainViewModel : ViewModelBase
{
    #region MEMBERS

    private readonly ObservableCollection<FormatItemViewModel> _d3dtxTypes =
    [
        new FormatItemViewModel { Name = "DDS", ItemStatus = true },
        new FormatItemViewModel { Name = "PNG", ItemStatus = true },
        new FormatItemViewModel { Name = "JPEG", ItemStatus = true },
        new FormatItemViewModel { Name = "BMP", ItemStatus = true },
        new FormatItemViewModel { Name = "TIFF", ItemStatus = true },
        new FormatItemViewModel { Name = "TGA", ItemStatus = true },
        new FormatItemViewModel { Name = "HDR", ItemStatus = true },
    ];

    private readonly ObservableCollection<FormatItemViewModel> _ddsTypes =
    [
        new FormatItemViewModel { Name = "D3DTX", ItemStatus = true },
        new FormatItemViewModel { Name = "PNG", ItemStatus = true },
        new FormatItemViewModel { Name = "JPEG", ItemStatus = true },
        new FormatItemViewModel { Name = "BMP", ItemStatus = true },
        new FormatItemViewModel { Name = "TIFF", ItemStatus = true },
        new FormatItemViewModel { Name = "TGA", ItemStatus = true },
        new FormatItemViewModel { Name = "HDR", ItemStatus = true },
    ];

    private readonly ObservableCollection<FormatItemViewModel> _otherTypes =
    [
        new FormatItemViewModel { Name = "D3DTX", ItemStatus = true },
    ];

    private readonly ObservableCollection<FormatItemViewModel> _folderTypes =
    [
        new FormatItemViewModel { Name = "D3DTX", ItemStatus = true },
        new FormatItemViewModel { Name = "DDS", ItemStatus = true },
        new FormatItemViewModel { Name = "PNG", ItemStatus = true },
        new FormatItemViewModel { Name = "JPEG", ItemStatus = true },
        new FormatItemViewModel { Name = "BMP", ItemStatus = true },
        new FormatItemViewModel { Name = "TIFF", ItemStatus = true },
        new FormatItemViewModel { Name = "TGA", ItemStatus = true },
        new FormatItemViewModel { Name = "HDR", ItemStatus = true },
    ];

    private readonly MainManager mainManager = MainManager.GetInstance();
    private readonly Uri _assetsUri = new("avares://TelltaleTextureTool/Assets/");
    private static readonly string ErrorSvgFilename = "error.svg";

    #endregion

    public WindowNotificationManager? NotificationManager { get; set; }

    #region UI PROPERTIES
    public ImageEffect[] ImageConversionModes { get; } =
        [
            ImageEffect.None,
            ImageEffect.SwizzleRB,
            ImageEffect.SwizzleRGBA,
            ImageEffect.RestoreZ,
            ImageEffect.RemoveZ,
        ];

    public Platform[] SwizzlePlatforms { get; } =
        [
            Platform.None,
            Platform.Xbox360,
            Platform.PS3,
            Platform.PS4,
            Platform.Switch,
            Platform.PSVita,
        ];

    public TelltaleToolGame[] Games { get; } =
        [
            TelltaleToolGame.DEFAULT,
            TelltaleToolGame.TEXAS_HOLD_EM_OG, // LV?
            TelltaleToolGame.TEXAS_HOLD_EM_V1, // LV9
            TelltaleToolGame.BONE_OUT_FROM_BONEVILLE, // LV11
            TelltaleToolGame.CSI_3_DIMENSIONS, // LV12
            TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_101_2006, // LV13
            TelltaleToolGame.BONE_THE_GREAT_COW_RACE, // LV11
            TelltaleToolGame.CSI_HARD_EVIDENCE, // LV10
            TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_201_OG, // LV9
            TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_201_NEW,
            TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_101, // LV8
            TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_102, // LV8
            TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_103, // LV7
            TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_104, // LV7
            TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_105, // LV6
            TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_101, // LV5
            TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_102, // LV5
            TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_103, // LV5
            TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_104, // LV4
            TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_101_2007, // LV4
            TelltaleToolGame.CSI_DEADLY_INTENT, // LV4
            TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V1, // LV4
            TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V2, // LV4
            TelltaleToolGame.CSI_FATAL_CONSPIRACY, // LV4
            TelltaleToolGame.NELSON_TETHERS_PUZZLE_AGENT, // LV3
            TelltaleToolGame.POKER_NIGHT_AT_THE_INVENTORY, // LV3
            TelltaleToolGame.SAM_AND_MAX_THE_DEVILS_PLAYHOUSE_301, // LV4
            TelltaleToolGame.BACK_TO_THE_FUTURE_THE_GAME, // LV3
            TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE, // LV3
            TelltaleToolGame.JURASSIC_PARK_THE_GAME, // LV2
            TelltaleToolGame.PUZZLE_AGENT_2, // LV2
            TelltaleToolGame.LAW_AND_ORDER_LEGACIES, // LV2
            TelltaleToolGame.THE_WALKING_DEAD, // LV1
        ];

    [ObservableProperty]
    private ImageProperties _imageProperties = new();

    [ObservableProperty]
    private ImageAdvancedOptions _imageAdvancedOptions;

    [ObservableProperty]
    private DataGridColumnVisibilitySettings _columnSettings = new();

    [ObservableProperty]
    private FormatItemViewModel _selectedFromFormat = new();

    [ObservableProperty]
    private FormatItemViewModel _selectedToFormat = new();

    [ObservableProperty]
    private ObservableCollection<FormatItemViewModel> _fromFormatsList = [];

    [ObservableProperty]
    private ObservableCollection<FormatItemViewModel> _toFormatsList = [];

    [ObservableProperty]
    private bool _isFromSelectedComboboxEnable;

    [ObservableProperty]
    private bool _isToSelectedComboboxEnable;

    [ObservableProperty]
    private bool _versionConvertComboBoxStatus;

    [ObservableProperty]
    private bool _saveButtonStatus;

    [ObservableProperty]
    private bool _deleteButtonStatus;

    [ObservableProperty]
    private bool _convertButtonStatus;

    [ObservableProperty]
    private bool _contextOpenFolderStatus;

    [ObservableProperty]
    private bool _chooseOutputDirectoryCheckBoxEnabledStatus;

    [ObservableProperty]
    private int _selectedComboboxIndex;

    [ObservableProperty]
    private int _selectedLegacyTitleIndex;

    [ObservableProperty]
    private uint _maxMipCountButton;

    [ObservableProperty]
    private IImage? _imagePreview;

    [ObservableProperty]
    private string _directoryPath = string.Empty;

    [ObservableProperty]
    private bool _returnDirectoryButtonStatus;

    [ObservableProperty]
    private bool _refreshDirectoryButtonStatus;

    [ObservableProperty]
    private bool _chooseOutputDirectoryCheckboxStatus;

    [ObservableProperty]
    private bool _isMipSliderVisible;

    [ObservableProperty]
    private bool _isFaceSliderVisible;

    [ObservableProperty]
    private bool _isSliceSliderVisible;

    [ObservableProperty]
    private bool _isImageInformationVisible = true;

    [ObservableProperty]
    private bool _isDebugInformationVisible = false;

    [ObservableProperty]
    private string _debugInfo = string.Empty;

    [ObservableProperty]
    private uint _mipValue;

    [ObservableProperty]
    private uint _faceValue;

    [ObservableProperty]
    private uint _maxMipCount;

    [ObservableProperty]
    private uint _maxFaceCount;

    [ObservableProperty]
    private uint _sliceValue;

    [ObservableProperty]
    private uint _maxSliceCount;

    [ObservableProperty]
    private static ObservableCollection<FileSystemItem> _workingDirectoryFiles = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor("ResetPanAndZoomCommand")]
    private FileSystemItem _dataGridSelectedItem = new();

    private Texture? texture;
    private readonly CodecManager codecManager = new();

    public class FormatItemViewModel
    {
        public string? Name { get; set; }
        public bool ItemStatus { get; set; }
    }

    public RelayCommand ResetPanAndZoomCommand { get; internal set; }

    public RelayCommand<Notification> DisplayErrorCommand { get; internal set; }

    #endregion

    private static FilePickerFileType FileFilterTypes = new("")
    {
        Patterns = [],
        AppleUniformTypeIdentifiers = [],
        MimeTypes = [],
    };

    public MainViewModel()
    {
        ImagePreview = new SvgImage() { Source = SvgSource.Load(ErrorSvgFilename, _assetsUri) };
        ImageAdvancedOptions = new ImageAdvancedOptions(this);

        FileFilterTypes = new FilePickerFileType("Supported Files")
        {
            Patterns = [.. codecManager.GetAllSupportedExtensions(), ".json"],
            AppleUniformTypeIdentifiers = ["public.image"],
            MimeTypes = ["image/*"],
        };
    }

    #region MAIN MENU BUTTONS ACTIONS


    private async Task<IStorageFolder?> DoOpenFolderPickerAsync()
    {
        // For learning purposes, we opted to directly get the reference
        // for StorageProvider APIs here inside the ViewModel.

        // For your real-world apps, you should follow the MVVM principles
        // by making service classes and locating them with DI/IoC.

        // See IoCFileOps project for an example of how to accomplish this.
        if (
            Application.Current?.ApplicationLifetime
                is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow?.StorageProvider is not { } provider
        )
            throw new NullReferenceException("Missing StorageProvider instance.");

        var folder = await provider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions()
            {
                Title = "Open Folder With D3DTX Files",
                AllowMultiple = false,
            }
        );

        return folder?.Count >= 1 ? folder[0] : null;
    }

    private async Task<IStorageFile?> DoOpenFilePickerAsync()
    {
        // For learning purposes, we opted to directly get the reference
        // for StorageProvider APIs here inside the ViewModel.

        // For your real-world apps, you should follow the MVVM principles
        // by making service classes and locating them with DI/IoC.

        // See IoCFileOps project for an example of how to accomplish this.
        if (
            Application.Current?.ApplicationLifetime
                is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow?.StorageProvider is not { } provider
        )
            throw new NullReferenceException("Missing StorageProvider instance.");

        var file = await provider.OpenFilePickerAsync(
            new FilePickerOpenOptions()
            {
                Title = "Open Folder With D3DTX Files",
                AllowMultiple = false,
            }
        );

        return file?.Count >= 1 ? file[0] : null;
    }

    // Open Directory Command
    [RelayCommand]
    public async Task OpenDirectoryButton_Click()
    {
        try
        {
            var folder = await DoOpenFolderPickerAsync();
            if (folder is null)
                return;

            //  mainManager.SetWorkingDirectoryPath(folder.TryGetLocalPath());
            WorkingDirectoryFiles = mainManager
                .GetWorkingDirectory()
                .GetFiles(
                    folder.TryGetLocalPath()
                        ?? throw new ArgumentNullException(nameof(folder), "Folder path is null"),
                    FileFilterTypes.Patterns
                );
            ReturnDirectoryButtonStatus = true;
            RefreshDirectoryButtonStatus = true;
            DataGridSelectedItem = null;
            UpdateUi();
        }
        catch (Exception e)
        {
            HandleException(e.Message);
        }
    }

    // Open Directory Command
    [RelayCommand]
    public async Task OpenFileButton_Click()
    {
        try
        {
            var file = await DoOpenFilePickerAsync();
            if (file is null)
                return;

            //  mainManager.SetWorkingDirectoryPath(folder.TryGetLocalPath());
            WorkingDirectoryFiles = mainManager
                .GetWorkingDirectory()
                .GetFiles(
                    Path.GetDirectoryName(file.TryGetLocalPath())
                        ?? throw new ArgumentNullException(nameof(file), "File path is null"),
                    FileFilterTypes.Patterns
                );
            ReturnDirectoryButtonStatus = true;
            RefreshDirectoryButtonStatus = true;
            DataGridSelectedItem =
                WorkingDirectoryFiles.FirstOrDefault(x => x.FullPath == file.TryGetLocalPath())
                ?? throw new ArgumentNullException(nameof(file), "File path is null");

            UpdateUi();
        }
        catch (Exception e)
        {
            HandleException(e.Message);
        }
    }

    [RelayCommand]
    public async Task SaveFileButton_Click()
    {
        try
        {
            if (DataGridSelectedItem is null)
                return;

            var topLevel = GetMainWindow();

            if (Directory.Exists(DataGridSelectedItem.FullPath))
            {
                throw new Exception("Cannot save a directory.");
            }

            // Start async operation to open the dialog.
            var storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = "Save File",
                    SuggestedFileName = DataGridSelectedItem.Name,
                    ShowOverwritePrompt = true,
                    DefaultExtension = DataGridSelectedItem.Type is null
                        ? "bin"
                        : DataGridSelectedItem.Type[1..],
                }
            );

            if (storageFile is null)
                return;

            var destinationFilePath = storageFile.Path.AbsolutePath;

            if (File.Exists(DataGridSelectedItem.FullPath))
                File.Copy(DataGridSelectedItem.FullPath, destinationFilePath, true);
        }
        catch (Exception ex)
        {
            HandleException("Error during saving the file. " + ex.Message);
        }
        finally
        {
            SafeRefreshDirectory();
            UpdateUi();
        }
    }

    [RelayCommand]
    public async Task AddFiles()
    {
        try
        {
            if (string.IsNullOrEmpty(DirectoryPath) || !Directory.Exists(DirectoryPath))
                return;

            var topLevel = GetMainWindow();

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions()
                {
                    Title = "Open Files",
                    AllowMultiple = true,
                    SuggestedStartLocation =
                        await topLevel.StorageProvider.TryGetFolderFromPathAsync(DirectoryPath),
                    FileTypeFilter = [FileFilterTypes],
                }
            );

            foreach (var file in files)
            {
                var destinationFilePath = Path.Combine(DirectoryPath, file.Name);

                var i = 1;
                while (File.Exists(destinationFilePath))
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                    var extension = Path.GetExtension(file.Name);
                    destinationFilePath = Path.Combine(
                        DirectoryPath,
                        $"{fileNameWithoutExtension}({i++}){extension}"
                    );
                }

                File.Copy(new Uri(file.Path.ToString()).LocalPath, destinationFilePath);
            }
        }
        catch (Exception ex)
        {
            HandleException("Error during adding files. Some files were not copied. " + ex.Message);
        }

        SafeRefreshDirectory();
        UpdateUi();
    }

    // Delete Command
    [RelayCommand]
    public async Task DeleteFile()
    {
        var workingDirectoryFile = DataGridSelectedItem;

        var textureFilePath = workingDirectoryFile.FullPath;

        try
        {
            if (File.Exists(textureFilePath))
            {
                File.Delete(textureFilePath);
            }
            else if (Directory.Exists(textureFilePath))
            {
                var mainWindow = GetMainWindow();
                var messageBox = MessageBoxes.GetConfirmationBox(
                    "Are you sure you want to delete this directory?"
                );

                var result = await MessageBoxManager
                    .GetMessageBoxStandard(messageBox)
                    .ShowWindowDialogAsync(mainWindow);

                if (result is not ButtonResult.Yes)
                    return;

                Directory.Delete(textureFilePath);
            }
            else
            {
                throw new Exception("Invalid file or directory path.");
            }
        }
        catch (Exception ex)
        {
            HandleException(ex.Message);
        }
        finally
        {
            DataGridSelectedItem = null;
            SafeRefreshDirectory();
            UpdateUi();
        }
    }

    [RelayCommand]
    public void HelpButton_Click()
    {
        MainManager.OpenAppHelp();
    }

    [RelayCommand]
    public void AboutButton_Click()
    {
        var mainWindow = GetMainWindow();
        var aboutWindow = new AboutWindow { DataContext = new AboutViewModel() };

        aboutWindow.ShowDialog(mainWindow);
    }

    #endregion

    #region CONTEXT MENU ACTIONS

    [RelayCommand]
    public void ContextMenuOpenFileCommand()
    {
        try
        {
            if (DataGridSelectedItem is null)
                return;

            var workingDirectoryFile = DataGridSelectedItem;

            var filePath = workingDirectoryFile.FullPath;

            if (!File.Exists(filePath) && !Directory.Exists(filePath))
                throw new DirectoryNotFoundException("Directory was not found");

            mainManager.OpenFile(filePath);
        }
        catch (Exception ex)
        {
            HandleException(ex.Message);
        }
    }

    [RelayCommand]
    public async Task ContextMenuOpenFolderCommand()
    {
        try
        {
            // if there is no valid item selected, don't continue
            if (DataGridSelectedItem is null)
                return;

            // get our selected file object from the working directory
            var workingDirectoryFile = DataGridSelectedItem;
            if (!Directory.Exists(workingDirectoryFile.FullPath))
                throw new DirectoryNotFoundException("Directory not found.");

            mainManager.SetWorkingDirectoryPath(workingDirectoryFile.FullPath);
            WorkingDirectoryFiles = mainManager
                .GetWorkingDirectory()
                .GetFiles(workingDirectoryFile.FullPath, FileFilterTypes.Patterns);
        }
        catch (Exception ex)
        {
            HandleException(ex.Message);
        }
        finally
        {
            ContextOpenFolderStatus = false;
            UpdateUi();
        }
    }

    [RelayCommand]
    public async Task ContextMenuOpenFileExplorerCommand()
    {
        try
        {
            if (DirectoryPath is null)
                return;

            if (DataGridSelectedItem is null)
            {
                if (Directory.Exists(DirectoryPath))
                    await OpenFileExplorer(DirectoryPath);
            }
            else
            {
                if (File.Exists(DataGridSelectedItem.FullPath))
                    await OpenFileExplorer(DataGridSelectedItem.FullPath);
                else if (Directory.Exists(DataGridSelectedItem.FullPath))
                    await OpenFileExplorer(DataGridSelectedItem.FullPath);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex.Message);
        }
    }

    [RelayCommand]
    public async Task RefreshDirectoryButton_Click()
    {
        if (DirectoryPath is not null && DirectoryPath != string.Empty)
        {
            await RefreshUiAsync();
        }
    }

    public void SafeRefreshDirectory()
    {
        try
        {
            mainManager.RefreshWorkingDirectory();
        }
        catch (Exception ex)
        {
            HandleException(ex.Message);
        }
    }

    #endregion

    #region CONVERTER PANEL ACTIONS

    /// <summary>
    /// Convert command of the "Convert to" button. It initiates the conversion process.
    /// Error dialogs appear when something goes wrong with the conversion process.
    /// </summary>
    [RelayCommand]
    public async Task ConvertButton_Click(IList selectedItems)
    {
        try
        {
            if (DataGridSelectedItem is null)
                return;

            string outputDirectoryPath = mainManager.GetWorkingDirectoryPath();

            if (ChooseOutputDirectoryCheckboxStatus)
            {
                var topLevel = GetMainWindow();

                // Start async operation to open the dialog.
                var folderPath = await topLevel.StorageProvider.OpenFolderPickerAsync(
                    new FolderPickerOpenOptions
                    {
                        Title = "Choose your output folder location.",
                        AllowMultiple = false,
                    }
                );

                if (folderPath is null || folderPath.Count is 0)
                {
                    return;
                }

                outputDirectoryPath = folderPath[0].Path.AbsolutePath;
            }

            string? textureFilePath = DataGridSelectedItem.FullPath;

            TextureType oldTextureType = GetTextureTypeFromItem(SelectedFromFormat.Name);
            TextureType newTextureType = GetTextureTypeFromItem(SelectedToFormat.Name);

            if (File.Exists(textureFilePath))
            {
                // Converter.ConvertTexture(
                //     textureFilePath,
                //     outputDirectoryPath,
                //     ImageAdvancedOptions,
                //     oldTextureType,
                //     newTextureType
                // );
                string finalTexturePath =
                    DataGridSelectedItem.Name + GetExtensionFromTextureType(newTextureType);

                string finalPath = Path.Combine(outputDirectoryPath, finalTexturePath);

                CodecOptions codecOptions = new()
                {
                    TelltaleToolGame = ImageAdvancedOptions.GameID,
                };

                Texture toConvertTexture = codecManager.LoadFromFile(
                    DataGridSelectedItem.FullPath,
                    codecOptions
                );

                // if (ImageAdvancedOptions.IsDeswizzle)
                // {
                //     toConvertTexture.SwizzleTexture(Platform.Switch, false);
                // }

                codecManager.SaveToFile(finalPath, toConvertTexture, codecOptions);
            }
            else if (Directory.Exists(textureFilePath))
            {
                if (!ChooseOutputDirectoryCheckboxStatus)
                {
                    outputDirectoryPath = textureFilePath;
                }

                if (
                    Converter.ConvertBulk(
                        textureFilePath,
                        outputDirectoryPath,
                        ImageAdvancedOptions,
                        oldTextureType,
                        newTextureType
                    )
                )
                {
                    var mainWindow = GetMainWindow();
                    var messageBox = MessageBoxes.GetSuccessBox(
                        "All textures have been converted successfully!"
                    );
                    await MessageBoxManager
                        .GetMessageBoxStandard(messageBox)
                        .ShowWindowDialogAsync(mainWindow);
                }
            }

            // Generate JSON file
        }
        catch (Exception ex)
        {
            HandleImagePreviewError(ex);
        }
        finally
        {
            UpdateUi();
        }
    }

    private static TextureType GetTextureTypeFromItem(string newTextureType)
    {
        return newTextureType switch
        {
            "D3DTX" => TextureType.D3DTX,
            "DDS" => TextureType.DDS,
            "PNG" => TextureType.PNG,
            "JPG" => TextureType.JPEG,
            "JPEG" => TextureType.JPEG,
            "BMP" => TextureType.BMP,
            "TIF" => TextureType.TIFF,
            "TIFF" => TextureType.TIFF,
            "TGA" => TextureType.TGA,
            "HDR" => TextureType.HDR,
            _ => TextureType.Unknown,
        };
    }

    private static string GetExtensionFromTextureType(TextureType textureType)
    {
        return textureType switch
        {
            TextureType.D3DTX => ".d3dtx",
            TextureType.DDS => ".dds",
            TextureType.PNG => ".png",
            TextureType.JPEG => ".jpg",
            TextureType.BMP => ".bmp",
            TextureType.TIFF => ".tiff",
            TextureType.TGA => ".tga",
            TextureType.HDR => ".hdr",
            _ => string.Empty,
        };
    }

    #endregion

    ///<summary>
    /// Updates our application UI, mainly the data grid.
    ///</summary>
    private void UpdateUi()
    {
        // Update our texture directory UI
        try
        {
            DirectoryPath = mainManager.GetWorkingDirectoryPath();

            WorkingDirectoryFiles = mainManager
                .GetWorkingDirectory()
                .UpdateFiles(WorkingDirectoryFiles);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            HandleException("Error during updating UI. " + ex.Message);
        }
    }

    #region SMALL MENU BUTTON ACTIONS

    [RelayCommand]
    public async Task ReturnDirectory_Click()
    {
        try
        {
            if (Directory.GetParent(DirectoryPath) is null)
                return;
            DirectoryPath = Directory.GetParent(DirectoryPath).ToString();
            WorkingDirectoryFiles = mainManager
                .GetWorkingDirectory()
                .GetFiles(DirectoryPath, FileFilterTypes.Patterns);
            DataGridSelectedItem = null;
        }
        catch (Exception ex)
        {
            HandleException(ex.Message);
        }
        finally
        {
            PreviewImage();
            UpdateUi();
        }
    }

    [RelayCommand]
    public async Task ContextMenuRefreshDirectoryCommand()
    {
        await RefreshDirectoryButton_Click();
    }

    #endregion

    #region HELPERS

    private static Window GetMainWindow()
    {
        if (
            Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime lifetime
        )
            return lifetime.MainWindow;

        throw new Exception("Main Parent Window Not Found");
    }

    private void ChangeComboBoxItemsByItemExtension(string itemExtension)
    {
        var extensionToMappings = new Dictionary<string, ObservableCollection<FormatItemViewModel>>
        {
            { ".dds", _ddsTypes },
            { ".d3dtx", _d3dtxTypes },
            { ".png", _otherTypes },
            { ".jpg", _otherTypes },
            { ".jpeg", _otherTypes },
            { ".bmp", _otherTypes },
            { ".tga", _otherTypes },
            { ".tif", _otherTypes },
            { ".tiff", _otherTypes },
            { ".hdr", _otherTypes },
            { string.Empty, _folderTypes },
        };

        if (itemExtension is null)
        {
            FromFormatsList = null;
            ToFormatsList = null;
            ConvertButtonStatus = false;
            IsFromSelectedComboboxEnable = false;
            IsToSelectedComboboxEnable = false;
            VersionConvertComboBoxStatus = false;
            SelectedToFormat = null;
            SelectedFromFormat = null;
        }
        else if (extensionToMappings.TryGetValue(itemExtension, out var selectedItems))
        {
            if (itemExtension.Equals(".d3dtx"))
                VersionConvertComboBoxStatus = true;
            else
                VersionConvertComboBoxStatus = false;

            FromFormatsList = _folderTypes;
            ToFormatsList = selectedItems;
            IsFromSelectedComboboxEnable = IsToSelectedComboboxEnable = true;

            if (itemExtension != string.Empty)
            {
                SelectedFromFormat = _folderTypes[GetFormatPosition(itemExtension)];
                IsFromSelectedComboboxEnable = false;
            }

            ConvertButtonStatus = true;

            // SelectedComboboxIndex = GetFormatPosition(itemExtension);
            // There is an issue in Avalonia relating to dynamic sources and binding indexes.
            // Github issue: https://github.com/AvaloniaUI/Avalonia/issues/13736
            // When fixed, the line below can be removed.
            SelectedToFormat = selectedItems[0];
        }
        else
        {
            FromFormatsList = null;
            ToFormatsList = null;
            ConvertButtonStatus = false;
            IsFromSelectedComboboxEnable = false;
            IsToSelectedComboboxEnable = false;
            VersionConvertComboBoxStatus = false;
            SelectedToFormat = null;
            SelectedFromFormat = null;
        }
    }

    private static int GetFormatPosition(string itemExtension)
    {
        TextureType textureType = TextureType.Unknown;

        if (itemExtension != string.Empty)
            textureType = GetTextureTypeFromItem(itemExtension.ToUpperInvariant().Remove(0, 1));

        return textureType switch
        {
            TextureType.D3DTX => 0,
            TextureType.DDS => 1,
            TextureType.PNG => 2,
            TextureType.JPEG => 3,
            TextureType.BMP => 4,
            TextureType.TIFF => 5,
            TextureType.TGA => 6,
            TextureType.HDR => 7,
            _ => 0,
        };
    }

    #endregion

    public async void RowDoubleTappedCommand(object? sender, TappedEventArgs args)
    {
        try
        {
            var source = args.Source;
            if (source is null)
                return;
            if (source is Border)
            {
                if (DataGridSelectedItem is null)
                    return;

                var workingDirectoryFile = DataGridSelectedItem;

                var filePath = workingDirectoryFile.FullPath;

                if (!File.Exists(filePath) && !Directory.Exists(filePath))
                    throw new DirectoryNotFoundException("Directory was not found");

                if (File.Exists(workingDirectoryFile.FullPath))
                {
                    mainManager.OpenFile(filePath);
                }
                else
                {
                    DirectoryPath = workingDirectoryFile.FullPath;
                    WorkingDirectoryFiles = mainManager
                        .GetWorkingDirectory()
                        .GetFiles(DirectoryPath, FileFilterTypes.Patterns);
                }
            }
        }
        catch (Exception ex)
        {
            HandleImagePreviewError(ex);
        }
        finally
        {
            ContextOpenFolderStatus = false;
        }
    }

    private void UpdateUIElementsAsync()
    {
        if (DataGridSelectedItem is not null)
        {
            var workingDirectoryFile = DataGridSelectedItem;
            var path = workingDirectoryFile.FullPath;
            var extension = Path.GetExtension(path).ToLowerInvariant();

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                ResetUIElements();
                mainManager.RefreshWorkingDirectory();
                UpdateUi();
                throw new Exception(
                    "File or directory do not exist anymore! Refreshing the directory."
                );
            }

            SaveButtonStatus = File.Exists(path);
            DeleteButtonStatus = true;
            ContextOpenFolderStatus = Directory.Exists(path);
            ChooseOutputDirectoryCheckBoxEnabledStatus = true;

            if (extension == string.Empty && !Directory.Exists(path))
            {
                ChangeComboBoxItemsByItemExtension(null);
                IsImageInformationVisible = false;
                IsDebugInformationVisible = false;
            }
            else
            {
                ChangeComboBoxItemsByItemExtension(extension);
                IsImageInformationVisible = extension != string.Empty;
                IsDebugInformationVisible = extension != string.Empty;
            }
        }
        else
        {
            ResetUIElements();
        }
    }

    private void ResetUIElements()
    {
        SaveButtonStatus = false;
        DeleteButtonStatus = false;
        ConvertButtonStatus = false;
        IsFromSelectedComboboxEnable = false;
        IsToSelectedComboboxEnable = false;
        VersionConvertComboBoxStatus = false;
        ChooseOutputDirectoryCheckBoxEnabledStatus = false;
        ChooseOutputDirectoryCheckboxStatus = false;

        ImageProperties = new ImageProperties();
        ImagePreview = new SvgImage() { Source = SvgSource.Load(ErrorSvgFilename, _assetsUri) };
        DebugInfo = string.Empty;

        IsFaceSliderVisible = MaxFaceCount != 0;
        IsMipSliderVisible = MaxMipCount != 0;
        IsSliceSliderVisible = MaxSliceCount != 0;
    }

    [RelayCommand]
    public void UpdateUIElementsOnItemChange()
    {
        PreviewImage();
        ResetPanAndZoomCommand.Execute(null);
    }

    [RelayCommand]
    public void PreviewImage()
    {
        try
        {
            UpdateUIElementsAsync();

            if (DataGridSelectedItem is null)
                return;

            var workingDirectoryFile = DataGridSelectedItem;
            var filePath = workingDirectoryFile.FullPath;
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            texture = null;
            GC.Collect();

            if (!codecManager.GetAllSupportedExtensions().Contains(extension))
            {
                ImageProperties = new ImageProperties { Name = workingDirectoryFile.Name };
                DebugInfo = string.Empty;
                UpdateBitmap();
                IsImageInformationVisible = false;
                return;
            }

            CodecOptions codecOptions = new() { TelltaleToolGame = ImageAdvancedOptions.GameID };

            texture = codecManager.LoadFromFile(filePath, codecOptions);

            var metadata = texture.Metadata;

            ImageProperties = new ImageProperties
            {
                Name = workingDirectoryFile.Name,
                Width = metadata.Width.ToString(),
                Height = metadata.Height.ToString(),
                Depth = metadata.Depth.ToString(),
                PixelFormat = metadata.PixelFormatInfo.PixelFormat.ToString(),
                SurfaceGamma = metadata.PixelFormatInfo.ColorSpace.ToString(),
                ArraySize = metadata.ArraySize.ToString(),
                MipMapCount = metadata.MipLevels.ToString(),
                TextureLayout = metadata.Dimension.ToString(),
                AlphaMode = metadata.IsPremultipliedAlpha ? "Premultiplied" : "Straight",
                IsCubemap = metadata.IsCubemap ? "Yes" : "No",
                IsVolumemap = metadata.IsVolumemap ? "Yes" : "No",
            };

            if (ImageAdvancedOptions.EnableSwizzle && ImageAdvancedOptions.IsDeswizzle)
            {
                texture.SwizzleTexture(ImageAdvancedOptions.PlatformType, false);
            }

            texture.ConvertToRGBA8();

            // Apply effects here
            MaxMipCount = metadata.MipLevels - 1;
            MaxFaceCount = metadata.ArraySize - 1;
            MaxSliceCount = metadata.Depth - 1;

            if (texture.Metadata.IsCubemap)
            {
                MaxFaceCount /= 6;
            }

            IsFaceSliderVisible = MaxFaceCount != 0;
            IsMipSliderVisible = MaxMipCount != 0;
            IsSliceSliderVisible = MaxSliceCount != 0;

            DebugInfo = texture.Metadata.ExtraMetadata.DebugInformation;

            UpdateBitmap();

            // ImageAdvancedOptions = ImageData.GetImageAdvancedOptions(ImageAdvancedOptions);

            // ImageData.Initialize(
            //     filePath,
            //     textureType,
            //     ImageAdvancedOptions.GameID,
            //     ImageAdvancedOptions.IsLegacyConsole
            // );

            // if (textureType is TextureType.Unknown)
            // {
            //     ImageData.Reset();
            // }

            // if (textureType is not TextureType.Unknown)
            // {
            //     ImageData.ApplyEffects(ImageAdvancedOptions);
            // }

            // MaxMipCountButton = ImageData.DDSImage.GetMaxMipLevels();
        }
        catch (Exception ex)
        {
            texture = null;
            UpdateBitmap();
            Console.WriteLine(ex.StackTrace);
            HandleImagePreviewError(ex);
        }
    }

    [RelayCommand]
    public void UpdateBitmap()
    {
        try
        {
            if (texture != null)
            {
                if (texture.Metadata.IsCubemap)
                {
                    ImagePreview = ImageData.GetBitmap(
                        texture.GetImage(MipValue, 0, 0).Width * 4,
                        texture.GetImage(MipValue, 0, 0).Height * 3,
                        texture.GetCubemapImage(0, MipValue)
                    );
                }
                else
                {
                    ImagePreview = ImageData.GetBitmap(
                        texture.GetImage(MipValue, FaceValue, SliceValue).Width,
                        texture.GetImage(MipValue, FaceValue, SliceValue).Height,
                        texture.GetRGBA8Pixels(MipValue, FaceValue, SliceValue)
                    );
                }
            }
            else
            {
                ImagePreview = new SvgImage
                {
                    Source = SvgSource.Load(ErrorSvgFilename, _assetsUri),
                };
            }
            // if (DataGridSelectedItem is null)
            //     return;

            // var workingDirectoryFile = DataGridSelectedItem;
            // var filePath = workingDirectoryFile.FilePath;
            // var extension = Path.GetExtension(filePath).ToLowerInvariant();

            // ImageData.ApplyEffects(ImageAdvancedOptions);

            // MaxMipCount = ImageData.MaxMip;
            // MaxFaceCount = ImageData.MaxFace;

            // IsFaceSliderVisible = MaxFaceCount != 0;
            // IsMipSliderVisible = MaxMipCount != 0;

            // ImageProperties = ImageData.ImageProperties;

            // if (textureType is not TextureType.Unknown)
            // {
            //     ImagePreview = ImageData.GetBitmapFromScratchImage(MipValue, FaceValue);
            // }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            HandleImagePreviewError(ex);
        }
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (
            e.PropertyName is nameof(MipValue)
            || e.PropertyName is nameof(FaceValue)
            || e.PropertyName is nameof(SliceValue)
            || e.PropertyName is nameof(ImageAdvancedOptions)
        )
        {
            if (e.PropertyName is nameof(MipValue))
            {
                MaxSliceCount = (uint)
                    Math.Max(0, (int)(texture.Metadata.Depth >> (int)MipValue) - 1);
                IsSliceSliderVisible = MaxSliceCount != 0;

                if (SliceValue > MaxSliceCount)
                {
                    SliceValue = MaxSliceCount;
                }
            }

            UpdateBitmap();
        }
        if (e.PropertyName is nameof(ColumnSettings.IsNameVisible))
        {
            Console.WriteLine(ColumnSettings.IsNameVisible);

            Console.WriteLine(ColumnSettings.IsExtensionVisible);

            Console.WriteLine(ColumnSettings.IsSizeVisible);

            Console.WriteLine(ColumnSettings.IsCreatedDateVisible);

            Console.WriteLine(ColumnSettings.IsCreatedDateVisible);
        }
    }

    private static Task OpenFileExplorer(string path)
    {
        MainManager.OpenFileExplorer(path);
        return Task.CompletedTask;
    }

    private async Task RefreshUiAsync()
    {
        SafeRefreshDirectory();
        UpdateUi();
    }

    private void HandleImagePreviewError(Exception ex)
    {
        Console.WriteLine(ex.StackTrace);
        HandleException(ex.Message);
        // ImagePreview = new SvgImage { Source = SvgSource.Load(ErrorSvgFilename, _assetsUri) };
        Console.WriteLine(ex.StackTrace);
        ImageProperties = new ImageProperties();
    }

    private void HandleException(string message)
    {
        NotificationManager?.Show(
            new Notification("Error", message, NotificationType.Error, TimeSpan.FromSeconds(5))
        );
    }
}
