using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Svg.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TelltaleTextureTool.Main;
using TelltaleTextureTool.Utilities;
using TelltaleTextureTool.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using IImage = Avalonia.Media.IImage;
using TelltaleTextureTool.DirectX;
using System.ComponentModel;
using Avalonia.Data.Converters;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.Graphics;

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
        DisplayAttribute attribute = field?.GetCustomAttributes(false)
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
                var attribute = field.GetCustomAttributes(false)
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
        new FormatItemViewModel { Name = "D3DTX", ItemStatus = true},
        new FormatItemViewModel { Name = "PNG", ItemStatus = true },
        new FormatItemViewModel { Name = "JPEG", ItemStatus = true },
        new FormatItemViewModel { Name = "BMP", ItemStatus = true },
        new FormatItemViewModel { Name = "TIFF", ItemStatus = true },
        new FormatItemViewModel { Name = "TGA", ItemStatus = true },
        new FormatItemViewModel { Name = "HDR", ItemStatus = true },
    ];

    private readonly ObservableCollection<FormatItemViewModel> _otherTypes =
        [
              new FormatItemViewModel { Name = "D3DTX", ItemStatus = true }
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

    private readonly List<string> _allTypes = [".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff", ".d3dtx", ".dds", ".hdr", ".tga"];

    // No idea if this is correct. This just sets a filter list.
    public static FilePickerFileType AllowedTypes { get; } = new("All Supported Types")
    {
        Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.tif", "*.tiff", "*.d3dtx", "*.dds", "*.hdr", "*.tga", "*.json"],
        AppleUniformTypeIdentifiers = ["public.image"],
        MimeTypes = ["image/png", "image/jpeg", "image/bmp", "image/tiff", "image/tga", "image/hdr", "image/vnd.ms-dds", "image/vnd.ms-d3dtx", "image/vnd.ms-ktx2"]
    };

    // No idea if this is correct
    public static FilePickerFileType AllowedTTarchTypes { get; } = new("TTArch Types")
    {
        Patterns = ["*.ttarch", "*.ttarch2"]
    };

    private readonly MainManager mainManager = MainManager.GetInstance();
    private readonly Uri _assetsUri = new("avares://TelltaleTextureTool/Assets/");
    private static readonly string ErrorSvgFilename = "error.svg";

    #endregion

    #region UI PROPERTIES
    public ImageEffect[] ImageConversionModes { get; } = [
        ImageEffect.DEFAULT,
        ImageEffect.SWIZZLE_ABGR,
        ImageEffect.RESTORE_Z,
        ImageEffect.REMOVE_Z
        ];

    public T3PlatformType[] SwizzlePlatforms { get; } = [
        T3PlatformType.ePlatform_All,
        T3PlatformType.ePlatform_Xbox,
        T3PlatformType.ePlatform_PS3,
        T3PlatformType.ePlatform_PS4,
        T3PlatformType.ePlatform_NX,
        T3PlatformType.ePlatform_Vita
        ];

    public TelltaleToolGame[] Games { get; } = [
     TelltaleToolGame.DEFAULT,
     TelltaleToolGame.TEXAS_HOLD_EM_OG,  // LV?
     TelltaleToolGame.TEXAS_HOLD_EM_V1,  // LV9
     TelltaleToolGame.BONE_OUT_FROM_BONEVILLE, // LV11
     TelltaleToolGame.CSI_3_DIMENSIONS, // LV12
     TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2006, // LV13
     TelltaleToolGame.BONE_THE_GREAT_COW_RACE, // LV11
     TelltaleToolGame.CSI_HARD_EVIDENCE, // LV10
     TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_OG, // LV9
     TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW,
     TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_101, // LV8
     TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_102, // LV8
     TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_103, // LV7
     TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_104, // LV7
     TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_105, // LV6
     TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_101, // LV5
     TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_102, // LV5
     TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_103, // LV5
     TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_104, // LV4
     TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2007, // LV4
     TelltaleToolGame.CSI_DEADLY_INTENT, // LV4
     TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V1, // LV4
     TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V2, // LV4
     TelltaleToolGame.CSI_FATAL_CONSPIRACY, // LV4
     TelltaleToolGame.NELSON_TETHERS_PUZZLE_AGENT, // LV3
     TelltaleToolGame.POKER_NIGHT_AT_THE_INVENTORY, // LV3
     TelltaleToolGame.SAM_AND_MAX_THE_DEVILS_PLAYHOUSE, // LV4
     TelltaleToolGame.BACK_TO_THE_FUTURE_THE_GAME, // LV3
     TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE, // LV3
     TelltaleToolGame.JURASSIC_PARK_THE_GAME, // LV2
     TelltaleToolGame.PUZZLE_AGENT_2, // LV2
     TelltaleToolGame.LAW_AND_ORDER_LEGACIES, // LV2
     TelltaleToolGame.THE_WALKING_DEAD, // LV1
        ];

    [ObservableProperty] private ImageProperties _imageProperties;
    [ObservableProperty] private ImageAdvancedOptions _imageAdvancedOptions;
    [ObservableProperty] private bool _isChecked = false;
    [ObservableProperty] private FormatItemViewModel _selectedFromFormat;
    [ObservableProperty] private FormatItemViewModel _selectedToFormat;
    [ObservableProperty] private ObservableCollection<FormatItemViewModel> _fromFormatsList = [];
    [ObservableProperty] private ObservableCollection<FormatItemViewModel> _toFormatsList = [];
    [ObservableProperty] private bool _isFromSelectedComboboxEnable;
    [ObservableProperty] private bool _isToSelectedComboboxEnable;
    [ObservableProperty] private bool _versionConvertComboBoxStatus;
    [ObservableProperty] private bool _saveButtonStatus;
    [ObservableProperty] private bool _deleteButtonStatus;
    [ObservableProperty] private bool _convertButtonStatus;
    [ObservableProperty] private bool _debugButtonStatus;
    [ObservableProperty] private bool _contextOpenFolderStatus;
    [ObservableProperty] private bool _chooseOutputDirectoryCheckBoxEnabledStatus;

    [ObservableProperty] private int _selectedComboboxIndex;
    [ObservableProperty] private int _selectedLegacyTitleIndex;
    [ObservableProperty] private uint _maxMipCountButton;
    [ObservableProperty] private string? _imageNamePreview;
    [ObservableProperty] private IImage? _imagePreview;
    [ObservableProperty] private string _fileText = string.Empty;
    [ObservableProperty] private string _directoryPath = string.Empty;
    [ObservableProperty] private bool _returnDirectoryButtonStatus;
    [ObservableProperty] private bool _refreshDirectoryButtonStatus;
    [ObservableProperty] private bool _chooseOutputDirectoryCheckboxStatus;
    [ObservableProperty] private bool _isMipSliderVisible;
    [ObservableProperty] private bool _isFaceSliderVisible;
    [ObservableProperty] private bool _isImageInformationVisible = true;
    [ObservableProperty] private bool _isDebugInformationVisible = false;
    [ObservableProperty] private string _debugInfo = string.Empty;
    [ObservableProperty] private uint _mipValue;
    [ObservableProperty] private uint _faceValue;
    [ObservableProperty] private uint _maxMipCount;
    [ObservableProperty] private uint _maxFaceCount;
    [ObservableProperty] private static ObservableCollection<WorkingDirectoryFile> _workingDirectoryFiles = [];
    [ObservableProperty] private ObservableCollection<WorkingDirectoryFile> _archiveFiles = [];
    [ObservableProperty] private ImageData _imageData = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor("ResetPanAndZoomCommand")]
    private WorkingDirectoryFile _dataGridSelectedItem = new();


    public class FormatItemViewModel
    {
        public string? Name { get; set; }
        public bool ItemStatus { get; set; }
    }

    public RelayCommand ResetPanAndZoomCommand { get; internal set; }

    private void ResetPanAndZoom()
    {
        // Logic to reset pan and zoom
        // This method will be linked with code-behind to reset the ZoomBorder.
    }

    #endregion

    public MainViewModel()
    {
        ImagePreview = new SvgImage()
        {
            Source = SvgSource.Load(ErrorSvgFilename, _assetsUri)
        };
        ImageAdvancedOptions = new ImageAdvancedOptions(this);
    }

    #region MAIN MENU BUTTONS ACTIONS

    // Open Directory Command
    [RelayCommand]
    public async Task OpenDirectoryButton_Click()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider instance.");

            await mainManager.SetWorkingDirectoryPath(provider);

            if (mainManager.GetWorkingDirectoryPath() != string.Empty)
            {
                ReturnDirectoryButtonStatus = true;
                RefreshDirectoryButtonStatus = true;
                DataGridSelectedItem = null;
                await UpdateUiAsync();
            }
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(e.Message);
        }
    }

    [RelayCommand]
    public async Task SaveFileButton_Click()
    {
        try
        {
            if (DataGridSelectedItem is not null)
            {
                var topLevel = GetMainWindow();

                if (Directory.Exists(DataGridSelectedItem.FilePath))
                {
                    throw new Exception("Cannot save a directory.");
                }

                // Start async operation to open the dialog.
                var storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save File",
                    SuggestedFileName = DataGridSelectedItem.FileName,
                    ShowOverwritePrompt = true,
                    DefaultExtension = DataGridSelectedItem.FileType is null ? "bin" : DataGridSelectedItem.FileType[1..]
                });

                if (storageFile is not null)
                {
                    var destinationFilePath = storageFile.Path.AbsolutePath;

                    if (File.Exists(DataGridSelectedItem.FilePath))
                        File.Copy(DataGridSelectedItem.FilePath, destinationFilePath, true);
                }
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync("Error during saving the file. " + ex.Message);
        }
        finally
        {
            await SafeRefreshDirectoryAsync();
            await UpdateUiAsync();
        }
    }

    [RelayCommand]
    public async Task AddFilesButton_Click()
    {
        try
        {
            if (string.IsNullOrEmpty(DirectoryPath) || !Directory.Exists(DirectoryPath)) return;

            var topLevel = GetMainWindow();

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open Files",
                AllowMultiple = true,
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(DirectoryPath),
                FileTypeFilter = [AllowedTypes]
            });

            foreach (var file in files)
            {
                var destinationFilePath = Path.Combine(DirectoryPath, file.Name);

                var i = 1;
                while (File.Exists(destinationFilePath))
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                    var extension = Path.GetExtension(file.Name);
                    destinationFilePath = Path.Combine(DirectoryPath,
                        $"{fileNameWithoutExtension}({i++}){extension}");
                }

                File.Copy(new Uri(file.Path.ToString()).LocalPath, destinationFilePath);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync("Error during adding files. Some files were not copied. " + ex.Message);
        }

        await SafeRefreshDirectoryAsync();
        await UpdateUiAsync();
    }

    // Delete Command
    [RelayCommand]
    public async Task DeleteFileButton_Click()
    {
        var workingDirectoryFile =
            DataGridSelectedItem;

        var textureFilePath = workingDirectoryFile.FilePath;

        try
        {
            if (File.Exists(textureFilePath))
            {
                File.Delete(textureFilePath);
            }
            else if (Directory.Exists(textureFilePath))
            {
                var mainWindow = GetMainWindow();
                var messageBox =
                    MessageBoxes.GetConfirmationBox("Are you sure you want to delete this directory?");

                var result = await MessageBoxManager.GetMessageBoxStandard(messageBox)
                    .ShowWindowDialogAsync(mainWindow);

                if (result is not ButtonResult.Yes) return;

                Directory.Delete(textureFilePath);
            }
            else
            {
                throw new Exception("Invalid file or directory path.");
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
        }
        finally
        {
            DataGridSelectedItem = null;
            await SafeRefreshDirectoryAsync();
            await UpdateUiAsync();
        }
    }

    [RelayCommand]
    public void HelpButton_Click()
    {
        mainManager.OpenAppHelp();
    }


    [RelayCommand]
    public void AboutButton_Click()
    {
        var mainWindow = GetMainWindow();
        var aboutWindow = new AboutWindow
        {
            DataContext = new AboutViewModel()
        };

        aboutWindow.ShowDialog(mainWindow);
    }

    #endregion

    #region CONTEXT MENU ACTIONS

    [RelayCommand]
    public async Task ContextMenuAddFilesCommand()
    {
        try
        {
            if (string.IsNullOrEmpty(DirectoryPath) || !Directory.Exists(DirectoryPath)) return;

            var topLevel = GetMainWindow();

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open Files",
                AllowMultiple = true
            });

            foreach (var file in files)
            {
                var destinationFilePath = Path.Combine(DirectoryPath, file.Name);

                var i = 1;
                while (File.Exists(destinationFilePath))
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                    var extension = Path.GetExtension(file.Name);
                    destinationFilePath = Path.Combine(DirectoryPath,
                        $"{fileNameWithoutExtension}({i++}){extension}");
                }

                File.Copy(file.Path.AbsolutePath, destinationFilePath);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync("Error during adding files. Some files were not copied." + ex.Message);
        }
        finally
        {

        }

        await SafeRefreshDirectoryAsync();
        await UpdateUiAsync();
    }

    [RelayCommand]
    public async Task ContextMenuOpenFileCommand()
    {
        try
        {
            if (DataGridSelectedItem is null)
                return;

            var workingDirectoryFile =
                DataGridSelectedItem;

            var filePath = workingDirectoryFile.FilePath;

            if (!File.Exists(filePath) && !Directory.Exists(filePath))
                throw new DirectoryNotFoundException("Directory was not found");

            mainManager.OpenFile(filePath);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
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
            if (!Directory.Exists(workingDirectoryFile.FilePath))
                throw new DirectoryNotFoundException("Directory not found.");

            await mainManager.SetWorkingDirectoryPath(workingDirectoryFile.FilePath);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
        }
        finally
        {
            ContextOpenFolderStatus = false;
            await UpdateUiAsync();
        }
    }

    [RelayCommand]
    public async Task ContextMenuOpenFileExplorerCommand()
    {
        try
        {
            if (DirectoryPath is null) return;

            if (DataGridSelectedItem is null)
            {
                if (Directory.Exists(DirectoryPath))
                    await OpenFileExplorer(DirectoryPath);
            }
            else
            {
                if (File.Exists(DataGridSelectedItem.FilePath))
                    await OpenFileExplorer(DataGridSelectedItem.FilePath);
                else if (Directory.Exists(DataGridSelectedItem.FilePath))
                    await OpenFileExplorer(DataGridSelectedItem.FilePath);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
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

    [RelayCommand]
    public async Task ContextDeleteFileCommand()
    {
        await DeleteFileButton_Click();
    }

    public async Task SafeRefreshDirectoryAsync()
    {
        try
        {
            mainManager.RefreshWorkingDirectory();
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
        }
    }

    #endregion

    #region CONVERTER PANEL ACTIONS

    /// <summary>
    /// Convert command of the "Convert to" button. It initiates the conversion process.
    /// Error dialogs appear when something goes wrong with the conversion process.
    /// </summary>
    [RelayCommand]
    public async Task ConvertButton_Click()
    {
        try
        {
            if (DataGridSelectedItem is null) return;

            var workingDirectoryFile =
                DataGridSelectedItem;

            string? textureFilePath = workingDirectoryFile.FilePath;

            if (!File.Exists(textureFilePath) && !Directory.Exists(textureFilePath))
                throw new DirectoryNotFoundException("File/Directory was not found.");

            string outputDirectoryPath = mainManager.GetWorkingDirectoryPath();

            if (ChooseOutputDirectoryCheckboxStatus)
            {
                var topLevel = GetMainWindow();

                // Start async operation to open the dialog.
                var folderPath = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Choose your output folder location.",
                    AllowMultiple = false,
                });

                if (folderPath is null || folderPath.Count is 0)
                {
                    return;
                }

                outputDirectoryPath = folderPath[0].Path.AbsolutePath;
            }

            TextureType oldTextureType = GetTextureTypeFromItem(SelectedFromFormat.Name);
            TextureType newTextureType = GetTextureTypeFromItem(SelectedToFormat.Name);

            if (File.Exists(textureFilePath))
            {
                Converter.ConvertTexture(textureFilePath, outputDirectoryPath, ImageAdvancedOptions, oldTextureType, newTextureType);
            }
            else if (Directory.Exists(textureFilePath))
            {
                if (!ChooseOutputDirectoryCheckboxStatus)
                {
                    outputDirectoryPath = textureFilePath;
                }

                if (Converter.ConvertBulk(textureFilePath, outputDirectoryPath, ImageAdvancedOptions, oldTextureType, newTextureType))
                {
                    var mainWindow = GetMainWindow();
                    var messageBox = MessageBoxes.GetSuccessBox("All textures have been converted successfully!");
                    await MessageBoxManager.GetMessageBoxStandard(messageBox).ShowWindowDialogAsync(mainWindow);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            var mainWindow = GetMainWindow();
            var messageBox =
                MessageBoxes.GetErrorBox(ex.Message);
            await MessageBoxManager.GetMessageBoxStandard(messageBox).ShowWindowDialogAsync(mainWindow);
            Logger.Log(ex);
        }
        finally
        {
            mainManager.RefreshWorkingDirectory();
            await UpdateUiAsync();
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
            _ => TextureType.Unknown
        };
    }

    /// <summary>
    /// Debug button command. It shows the debug information of the selected texture.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DebugButton_Click()
    {
        try
        {
            if (DataGridSelectedItem is null) return;

            var workingDirectoryFile =
                DataGridSelectedItem;

            string? textureFilePath = workingDirectoryFile.FilePath;

            string debugInfo = string.Empty;

            if (workingDirectoryFile.FileType is ".d3dtx")
            {
                var d3dtx = new D3DTX_Master();
                d3dtx.ReadD3DTXFile(textureFilePath, ImageAdvancedOptions.GameID, ImageAdvancedOptions.IsLegacyConsole);

                debugInfo = d3dtx.GetD3DTXDebugInfo();
            }
            else if (workingDirectoryFile.FileType is ".dds" or ".png" or ".jpg" or ".jpeg" or ".bmp" or ".tga" or ".tif" or ".tiff" or ".hdr")
            {
                TextureType textureType = GetTextureTypeFromItem(workingDirectoryFile.FileType.ToUpperInvariant().Remove(0, 1));
                debugInfo = TextureManager.GetTextureDebugInfo(textureFilePath, textureType);
            }
            else
            {
                debugInfo = string.Empty;
            }

            DebugInfo = debugInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            await HandleExceptionAsync(ex.Message);
        }
    }

    #endregion

    ///<summary>
    /// Updates our application UI, mainly the data grid.
    ///</summary>
    private async Task UpdateUiAsync()
    {
        // Update our texture directory UI
        try
        {
            DirectoryPath = mainManager.GetWorkingDirectoryPath();
            mainManager.RefreshWorkingDirectory();
            var workingDirectoryFiles = mainManager.GetWorkingDirectoryFiles();

            for (int i = WorkingDirectoryFiles.Count - 1; i >= 0; i--)
            {
                if (!workingDirectoryFiles.Contains(WorkingDirectoryFiles[i]))
                {
                    WorkingDirectoryFiles.RemoveAt(i);
                }
            }

            // Add items from the list to the observable collection if they are not already present
            foreach (var item in workingDirectoryFiles)
            {
                if (!WorkingDirectoryFiles.Contains(item))
                {
                    WorkingDirectoryFiles.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            await HandleExceptionAsync("Error during updating UI. " + ex.Message);
        }
    }

    #region SMALL MENU BUTTON ACTIONS

    [RelayCommand]
    public async Task ReturnDirectory_Click()
    {
        try
        {
            if (Directory.GetParent(DirectoryPath) is null) return;
            WorkingDirectoryFiles.Clear();
            await mainManager.SetWorkingDirectoryPath(Directory.GetParent(DirectoryPath).ToString());
            DataGridSelectedItem = null;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
        }
        finally
        {
            await PreviewImage();
            await UpdateUiAsync();
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
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
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
            {string.Empty, _folderTypes}
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
            _ => 0
        };
    }

    #endregion

    public async void RowDoubleTappedCommand(object? sender, TappedEventArgs args)
    {
        try
        {
            var source = args.Source;
            if (source is null) return;
            if (source is Border)
            {
                if (DataGridSelectedItem is null)
                    return;

                var workingDirectoryFile =
                    DataGridSelectedItem;

                var filePath = workingDirectoryFile.FilePath;

                if (!File.Exists(filePath) && !Directory.Exists(filePath))
                    throw new DirectoryNotFoundException("Directory was not found");

                if (File.Exists(workingDirectoryFile.FilePath))
                {
                    mainManager.OpenFile(filePath);
                }
                else
                {
                    await mainManager.SetWorkingDirectoryPath(workingDirectoryFile.FilePath);
                    WorkingDirectoryFiles.Clear();
                    await UpdateUiAsync();
                }
            }
        }
        catch (Exception ex)
        {
            var mainWindow = GetMainWindow();
            var messageBox = MessageBoxes.GetErrorBox(ex.Message);

            await MessageBoxManager.GetMessageBoxStandard(messageBox).ShowWindowDialogAsync(mainWindow);
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
            var path = workingDirectoryFile.FilePath;
            var extension = Path.GetExtension(path).ToLowerInvariant();

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                ResetUIElements();
                mainManager.RefreshWorkingDirectory();
                UpdateUiAsync().Wait();
                throw new Exception("File or directory do not exist anymore! Refreshing the directory.");
            }

            DebugButtonStatus = extension is ".d3dtx" || extension is ".dds";
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
        DebugButtonStatus = false;
        ConvertButtonStatus = false;
        IsFromSelectedComboboxEnable = false;
        IsToSelectedComboboxEnable = false;
        VersionConvertComboBoxStatus = false;
        ChooseOutputDirectoryCheckBoxEnabledStatus = false;
        DebugButtonStatus = false;
        ChooseOutputDirectoryCheckboxStatus = false;

        ImageProperties = new ImageProperties();
        ImagePreview = new SvgImage()
        {
            Source = SvgSource.Load(ErrorSvgFilename, _assetsUri)
        };
        ImageNamePreview = string.Empty;

        ImageData.Reset();

        MaxMipCount = ImageData.MaxMip;
        MaxFaceCount = ImageData.MaxFace;

        IsFaceSliderVisible = MaxFaceCount != 0;
        IsMipSliderVisible = MaxMipCount != 0;
    }

    [RelayCommand]
    public async Task UpdateUIElementsOnItemChange()
    {
        await PreviewImage();
        ResetPanAndZoomCommand.Execute(null);
    }

    [RelayCommand]
    public async Task PreviewImage()
    {
        try
        {
            UpdateUIElementsAsync();

            if (DataGridSelectedItem is null)
                return;

            var workingDirectoryFile = DataGridSelectedItem;
            var filePath = workingDirectoryFile.FilePath;
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (Directory.Exists(filePath))
            {
                return;
            }

            ImageNamePreview = workingDirectoryFile.FileName + workingDirectoryFile.FileType;

            TextureType textureType = TextureType.Unknown;
            if (extension != string.Empty)
                textureType = GetTextureTypeFromItem(extension.ToUpperInvariant().Remove(0, 1));

            ImageData.Initialize(filePath, textureType, ImageAdvancedOptions.GameID, ImageAdvancedOptions.IsLegacyConsole);

            if (textureType is TextureType.Unknown)
            {
                ImageData.Reset();
            }

            ImageAdvancedOptions = ImageData.GetImageAdvancedOptions(ImageAdvancedOptions);

            if (textureType is not TextureType.Unknown)
            {
                ImageData.ApplyEffects(ImageAdvancedOptions);
            }

            await DebugButton_Click();

            MaxMipCount = ImageData.MaxMip;
            MaxFaceCount = ImageData.MaxFace;

            IsFaceSliderVisible = MaxFaceCount != 0;
            IsMipSliderVisible = MaxMipCount != 0;

            MaxMipCountButton = ImageData.DDSImage.GetMaxMipLevels();

            ImageProperties = ImageData.ImageProperties;

            if (textureType is not TextureType.Unknown)
            {
                ImagePreview = ImageData.GetBitmapFromScratchImage(MipValue, FaceValue);
            }
            else
            {
                ImagePreview = new SvgImage
                {
                    Source = SvgSource.Load(ErrorSvgFilename, _assetsUri)
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            await HandleImagePreviewErrorAsync(ex);
        }
    }

    [RelayCommand]
    public async Task UpdateBitmap()
    {
        try
        {
            if (DataGridSelectedItem is null)
                return;

            var workingDirectoryFile = DataGridSelectedItem;
            var filePath = workingDirectoryFile.FilePath;
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            TextureType textureType = TextureType.Unknown;
            if (extension != string.Empty)
                textureType = GetTextureTypeFromItem(extension.ToUpperInvariant().Remove(0, 1));

            if (textureType is TextureType.Unknown)
            {
                return;
            }

            ImageData.ApplyEffects(ImageAdvancedOptions);

            MaxMipCount = ImageData.MaxMip;
            MaxFaceCount = ImageData.MaxFace;

            IsFaceSliderVisible = MaxFaceCount != 0;
            IsMipSliderVisible = MaxMipCount != 0;

            ImageProperties = ImageData.ImageProperties;

            if (textureType is not TextureType.Unknown)
            {
                ImagePreview = ImageData.GetBitmapFromScratchImage(MipValue, FaceValue);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            await HandleImagePreviewErrorAsync(ex);
        }
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(MipValue) || e.PropertyName is nameof(FaceValue))
        {
            if (ImageData.CurrentTextureType is not TextureType.Unknown)
                ImagePreview = ImageData.GetBitmapFromScratchImage(MipValue, FaceValue);
            else { ImagePreview = new SvgImage { Source = SvgSource.Load(ErrorSvgFilename, _assetsUri) }; }
        }
        if (e.PropertyName is nameof(ImageAdvancedOptions))
        {
            await UpdateBitmap();
        }
    }

    private static Task OpenFileExplorer(string path)
    {
        MainManager.OpenFileExplorer(path);
        return Task.CompletedTask;
    }

    private async Task RefreshUiAsync()
    {
        await SafeRefreshDirectoryAsync();
        await UpdateUiAsync();
    }

    private async Task HandleImagePreviewErrorAsync(Exception ex)
    {
        await HandleExceptionAsync("Error during previewing image.\nError message: " + ex.Message);
        ImagePreview = new SvgImage { Source = SvgSource.Load(ErrorSvgFilename, _assetsUri) };
        ImageProperties = new ImageProperties();
    }

    private static async Task HandleExceptionAsync(string message)
    {
        var mainWindow = GetMainWindow();
        var messageBox = MessageBoxes.GetErrorBox(message);
        await MessageBoxManager.GetMessageBoxStandard(messageBox).ShowWindowDialogAsync(mainWindow);
    }
}