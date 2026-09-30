using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using TelltaleTextureTool.Codecs;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.Utilities;
using TelltaleTextureTool.Views;
using TelltaleToolKit;
using static TelltaleTextureTool.ViewModels.ImagePropertiesViewModel;
using IImage = Avalonia.Media.IImage;
using Texture = TelltaleTextureTool.Graphics.Texture;

namespace TelltaleTextureTool.ViewModels;

public partial class MainViewModel : ObservableObject
{
    #region MEMBERS

    public partial class FormatItemViewModel(string name, TextureType type) : ObservableObject
    {
        public string Name { get; } = name;
        public TextureType Type { get; } = type;

        [ObservableProperty]
        private bool _isVisible = true;
    }

    //public AppConfig Config { get; } = ConfigHelper.Load();

    //[RelayCommand]
    //private void SaveBeforeExit()
    //{
    //    ConfigHelper.Save(Config);
    //}

    private readonly ObservableCollection<FormatItemViewModel> _conversionTypes =
    [
        new FormatItemViewModel("D3DTX", TextureType.D3DTX),
        new FormatItemViewModel("DDS", TextureType.DDS),
        new FormatItemViewModel("PNG", TextureType.PNG),
        new FormatItemViewModel("JPEG", TextureType.JPEG),
        new FormatItemViewModel("BMP", TextureType.BMP),
        new FormatItemViewModel("TIFF", TextureType.TIFF),
        new FormatItemViewModel("TGA", TextureType.TGA),
        new FormatItemViewModel("HDR", TextureType.HDR),
    ];

    [ObservableProperty]
    private FileExplorerViewModel _fileExplorerContext;

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
        TelltaleToolGame.NONE,
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
    private ImagePropertiesViewModel _imagePropertiesContext = new();

    [ObservableProperty]
    private FlatTreeDataGridSource<PropertyItem> _imagePropertiesContextSource;

    [ObservableProperty]
    private ConverterOptions _converterOptions;

    [ObservableProperty]
    private FormatItemViewModel _inputFormat;

    [ObservableProperty]
    private FormatItemViewModel _outputFormat;

    [ObservableProperty]
    private ObservableCollection<FormatItemViewModel> _inputFormatsList = [];

    [ObservableProperty]
    private ObservableCollection<FormatItemViewModel> _outputFormatsList = [];

    [ObservableProperty]
    private bool _isOutputFormatComboboxEnabled;

    [ObservableProperty]
    private bool _isInputFormatComboboxEnabled;

    [ObservableProperty]
    private bool _saveButtonStatus;

    [ObservableProperty]
    private bool _deleteButtonStatus;

    [ObservableProperty]
    private bool _hasImage;

    [ObservableProperty]
    private bool _convertButtonStatus;

    [ObservableProperty]
    private int _selectedLegacyTitleIndex;

    [ObservableProperty]
    private uint _maxMipCountButton;

    [ObservableProperty]
    private bool _isDebugInfoVisible = false;

    [ObservableProperty]
    private IImage? _imagePreview;

    [ObservableProperty]
    private bool _returnDirectoryButtonStatus;

    [ObservableProperty]
    private bool _isChooseOutputDirectoryCheckBoxEnabled;

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

    private Texture? texture;
    private readonly CodecManager codecManager = new();

    public RelayCommand ResetPanAndZoomCommand { get; internal set; }

    public RelayCommand<Notification> DisplayErrorCommand { get; internal set; }

    #endregion

    private readonly FilePickerFileType FileFilterTypes;

    public MainViewModel()
    {
        TTKContext.Instance().Load("data");

        ConverterOptions = new ConverterOptions(this);

        var fileFilters = codecManager.GetAllSupportedExtensions().Append(".json").ToList();

        FileFilterTypes = new FilePickerFileType("Supported Files")
        {
            Patterns = fileFilters.Select(x => x.StartsWith('.') ? "*" + x : x).ToImmutableList(),
            AppleUniformTypeIdentifiers = ["public.image"],
            MimeTypes = ["image/*"],
        };

        //var initDirectory = Directory.Exists(Config.LastFolder)
        //    ? Config.LastFolder
        //    : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        FileExplorerContext =
            new FileExplorerViewModel(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), fileFilters);

        FileExplorerContext.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(FileExplorerContext.SelectedItem))
            {
                PreviewImage();
                ResetPanAndZoomCommand.Execute(null);
                if (FileExplorerContext.SelectedItem is not null)
                {
                    //   DataGridSelectedItem = FileExplorerContext.SelectedItem;
                    //  UpdateUIElementsAsync();
                }
                else
                {
                    //  DataGridSelectedItem = null;
                    //  ResetUIElements();
                }
            }
        };

        ImagePropertiesContext = new ImagePropertiesViewModel();
        _imagePropertiesContextSource = ImagePropertiesContext.Source;
    }

    public enum BackgroundType
    {
        Transparent,
        Checkerboard,
        White,
        Black,
    }

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private BackgroundType _imageBackground = BackgroundType.Transparent;

    #region MAIN MENU BUTTONS ACTIONS

    private static async Task<IStorageFolder?> DoOpenFolderPickerAsync()
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

    // [ObservableProperty]
    // public bool _hasImage => ImagePreview != null;

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
                FileTypeFilter = [FileFilterTypes],
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

            FileExplorerContext.CurrentDirectory =
                folder.TryGetLocalPath()
                ?? throw new ArgumentNullException(nameof(folder), "File path is null");
            FileExplorerContext.SelectedItem = null;
        }
        catch (Exception ex)
        {
            HandleException(ex);
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

            FileExplorerContext.CurrentDirectory =
                Path.GetDirectoryName(file.TryGetLocalPath())
                ?? throw new ArgumentNullException(nameof(file), "File path is null");

            FileExplorerContext.SelectedItem =
                FileExplorerContext.FilteredDirectoryFiles.FirstOrDefault(x =>
                    x.Path.Equals(file.TryGetLocalPath(), StringComparison.OrdinalIgnoreCase)
                );
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    [RelayCommand]
    public async Task SaveFileButton_Click()
    {
        try
        {
            var DataGridSelectedItem = FileExplorerContext.SelectedItem;
            if (DataGridSelectedItem is null)
                return;

            var topLevel = GetMainWindow();

            if (Directory.Exists(DataGridSelectedItem.Path))
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

            if (File.Exists(DataGridSelectedItem.Path))
                File.Copy(DataGridSelectedItem.Path, destinationFilePath, true);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    [RelayCommand]
    public async Task AddFiles()
    {
        try
        {
            var topLevel = GetMainWindow();

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Select Files",
                    AllowMultiple = true,
                    FileTypeFilter = [FileFilterTypes],
                }
            );

            if (files is null || files.Count == 0)
                return;

            foreach (var file in files)
            {
                File.Copy(
                    file.TryGetLocalPath() ?? throw new ArgumentNullException(nameof(file)),
                    Path.Combine(FileExplorerContext.CurrentDirectory, file.Name),
                    true
                );
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    // Delete Command
    [RelayCommand]
    public void DeleteFile()
    {
        try
        {
            FileExplorerContext.DeleteFileCommand.Execute(null);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    [RelayCommand]
    public static void HelpButton_Click()
    {
        const string AppHelpLink =
            "https://github.com/Telltale-Modding-Group/Telltale-Texture-Tool/wiki";

        Process.Start(new ProcessStartInfo(AppHelpLink) { UseShellExecute = true });
    }

    [RelayCommand]
    public static void AboutButton_Click()
    {
        var mainWindow = GetMainWindow();
        var aboutWindow = new AboutWindow { DataContext = new AboutViewModel() };

        aboutWindow.ShowDialog(mainWindow);
    }

    #endregion

    #region CONTEXT MENU ACTIONS

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
            FileItem? DataGridSelectedItem = FileExplorerContext.SelectedItem;
            if (DataGridSelectedItem is null)
                return;

            string outputDirectoryPath = FileExplorerContext.CurrentDirectory;

            if (IsChooseOutputDirectoryCheckBoxEnabled)
            {
                Window topLevel = GetMainWindow();

                // Start async operation to open the dialog.
                IReadOnlyList<IStorageFolder>? folderPath = await topLevel.StorageProvider.OpenFolderPickerAsync(
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

            string? textureFilePath = DataGridSelectedItem.Path;

            TextureType oldTextureType = GetTextureTypeFromItem(InputFormat.Name);
            TextureType newTextureType = GetTextureTypeFromItem(OutputFormat.Name);

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
                    TelltaleToolGame = ConverterOptions.GameID,
                };

                Texture toConvertTexture = codecManager.LoadFromFile(
                    DataGridSelectedItem.Path,
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
                if (!IsChooseOutputDirectoryCheckBoxEnabled)
                {
                    outputDirectoryPath = textureFilePath;
                }

                if (
                    Converter.ConvertBulk(
                        textureFilePath,
                        outputDirectoryPath,
                        ConverterOptions,
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
        InputFormatsList = _conversionTypes;
        OutputFormatsList = _conversionTypes;

        foreach (var format in _conversionTypes)
        {
            format.IsVisible = true;
        }

        if (string.IsNullOrEmpty(itemExtension))
        {
            // Folder case - all formats visible
            IsInputFormatComboboxEnabled = IsOutputFormatComboboxEnabled = true;
            ConvertButtonStatus = true;
            return;
        }

        var inputType = GetTextureTypeFromItem(itemExtension.ToUpperInvariant().TrimStart('.'));
        InputFormat =
            _conversionTypes.FirstOrDefault(f => f.Type == inputType) ?? _conversionTypes[0];

        if (InputFormat == null)
        {
            IsInputFormatComboboxEnabled = IsOutputFormatComboboxEnabled = false;
            return;
        }

        OutputFormat = _conversionTypes.FirstOrDefault(f => f.IsVisible);

        IsInputFormatComboboxEnabled = false;
        IsOutputFormatComboboxEnabled = true;
        ConvertButtonStatus = true;
    }

    #endregion

    private void UpdateUIElementsAsync()
    {
        var selectedFile = FileExplorerContext.SelectedItem;

        if (selectedFile is not null)
        {
            var workingDirectoryFile = selectedFile;
            var path = workingDirectoryFile.Path;
            var extension = Path.GetExtension(path).ToLowerInvariant();

            SaveButtonStatus = File.Exists(path);
            DeleteButtonStatus = true;
            IsChooseOutputDirectoryCheckBoxEnabled = true;

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
        IsInputFormatComboboxEnabled = false;
        IsOutputFormatComboboxEnabled = false;
        IsChooseOutputDirectoryCheckBoxEnabled = false;
        //     ImageProperties = new ImageProperties();
        DebugInfo = string.Empty;

        ImagePreview = null;
        HasImage = false;

        texture = null;

        ResetTextureValues();
    }

    public void ResetTextureValues()
    {
        MipValue = 0;
        FaceValue = 0;
        SliceValue = 0;
        MaxMipCount = 0;
        MaxFaceCount = 0;
        MaxSliceCount = 0;
        IsFaceSliderVisible = MaxFaceCount != 0;
        IsMipSliderVisible = MaxMipCount != 0;
        IsSliceSliderVisible = MaxSliceCount != 0;
    }

    [RelayCommand]
    public void PreviewImage()
    {
        try
        {
            UpdateUIElementsAsync();

            if (FileExplorerContext.SelectedItem is null)
            {
                return;
            }

            texture = null;
            HasImage = false;
            DebugInfo = string.Empty;
            GC.Collect();

            var workingDirectoryFile = FileExplorerContext.SelectedItem;
            var filePath = workingDirectoryFile.Path;
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (!codecManager.GetAllSupportedExtensions().Contains(extension))
            {
                //        ImageProperties = new ImageProperties { Name = workingDirectoryFile.Name };
                UpdateBitmap();
                IsImageInformationVisible = false;
                return;
            }

            CodecOptions codecOptions = new() { TelltaleToolGame = ConverterOptions.GameID };

            texture = codecManager.LoadFromFile(filePath, codecOptions);

            StringBuilder pixelFormatInfo = new();
            pixelFormatInfo.AppendJoin(
                " ",
                $"{texture.Metadata.PixelFormatInfo.PixelFormat}",
                $"({texture.Metadata.PixelFormatInfo.DataType})",
                $"({texture.Metadata.PixelFormatInfo.ColorSpace})"
            );

            var metadata = texture.Metadata;

            ImagePropertiesContext.Properties[0].Value = workingDirectoryFile.Name;
            ImagePropertiesContext.Properties[1].Value =
                metadata.Depth == 1
                    ? $"{metadata.Width} × {metadata.Height}"
                    : $"{metadata.Width} × {metadata.Height} × {metadata.Depth}";
            ImagePropertiesContext.Properties[2].Value = pixelFormatInfo.ToString();
            ImagePropertiesContext.Properties[3].Value = metadata.MipLevels.ToString();
            ImagePropertiesContext.Properties[4].Value = metadata.ArraySize.ToString();
            ImagePropertiesContext.Properties[5].Value = metadata.IsPremultipliedAlpha
                ? "Premultiplied"
                : "Straight";
            ImagePropertiesContext.Properties[6].Value = metadata.IsCubemap ? "Yes" : "No";
            ImagePropertiesContext.Properties[7].Value = metadata.IsVolumemap ? "Yes" : "No";

            texture.SwizzleTexture(ConverterOptions.PlatformType, false);
            texture.ConvertToRGBA8();

            // Apply effects here
            MaxMipCount = metadata.MipLevels - 1;
            MaxFaceCount = metadata.ArraySize - 1;
            MaxSliceCount = metadata.Depth - 1;

            if (texture.Metadata.IsCubemap)
            {
                MaxFaceCount /= 6;
            }

            if (FileExplorerContext.ColumnSettings.IsMipSliderVisible)
            {
                IsMipSliderVisible = MaxMipCount != 0;
            }
            else
            {
                IsMipSliderVisible = false;
                MipValue = 0;
            }

            IsFaceSliderVisible = MaxFaceCount != 0;
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

            MaxMipCountButton = texture.GetMaxPossibleMipCount();
        }
        catch (Exception ex)
        {
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
                HasImage = true;
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
                HasImage = false;
                ResetTextureValues();
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

    [RelayCommand]
    private void ToggleMipSliderVisibility(bool isVisible)
    {
        if (!isVisible)
        {
            MipValue = 0;
            IsMipSliderVisible = false;
            UpdateBitmap();
        }
        else
        {
            IsMipSliderVisible = MaxMipCount != 0;
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        // if (e.PropertyName is nameof(ColumnSettings.IsMipSliderVisible))
        // {
        //     if (!ColumnSettings.IsMipSliderVisible)
        //     {
        //         IsMipSliderVisible = false;
        //         MipValue = 0;
        //         UpdateBitmap();
        //     }
        //     else
        //     {
        //         IsMipSliderVisible = MaxMipCount != 0;
        //     }
        // }
        if (e.PropertyName is nameof(FileExplorerContext.ColumnSettings.IsSizeVisible))
        {
            Console.WriteLine("Size column visibility changed.");
        }

        if (
            e.PropertyName is nameof(MipValue) or nameof(FaceValue) or nameof(SliceValue)
            or nameof(ConverterOptions)
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
    }

    private void HandleImagePreviewError(Exception ex)
    {
        HandleException(ex);
        //   ImageProperties = new ImageProperties();
    }

    private void HandleException(Exception ex)
    {
        Console.WriteLine(ex.StackTrace);
        NotificationManager?.Show(
            new Notification("Error", ex.Message, NotificationType.Error, TimeSpan.FromSeconds(5))
        );
    }
}