using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TelltaleTextureTool.Codecs;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.ViewModels;

namespace TelltaleTextureTool;

public partial class ImageAdvancedOptions : ObservableObject
{
    // Private telltale games
    [ObservableProperty]
    private TelltaleToolGame _gameID = TelltaleToolGame.DEFAULT;

    [ObservableProperty]
    private TextureType _textureType;

    [ObservableProperty]
    private bool _enableMips = false;

    [ObservableProperty]
    private bool _autoGenerateMips = false;

    [ObservableProperty]
    private bool _manualGenerateMips = false;

    [ObservableProperty]
    private uint _setMips = 1;

    [ObservableProperty]
    private bool _compression;

    [ObservableProperty]
    private bool _enableAutomaticCompression = true;

    [ObservableProperty]
    private bool _isAutomaticCompression;

    [ObservableProperty]
    private bool _enableTelltaleNormalMap = true;

    [ObservableProperty]
    private bool _isTelltaleNormalMap = false;

    [ObservableProperty]
    private bool _isTelltaleXYNormalMap = false;

    [ObservableProperty]
    private bool _isSRGB = false;

    [ObservableProperty]
    private T3SurfaceFormat _format;

    [ObservableProperty]
    private bool _enableNormalMap;

    [ObservableProperty]
    private bool _encodeDDSHeader;

    [ObservableProperty]
    private bool _filterValues;

    [ObservableProperty]
    private bool _enableWrapU;

    [ObservableProperty]
    private bool _enableWrapV;

    [ObservableProperty]
    private bool _enableEditing;

    [ObservableProperty]
    private bool _enableSwizzle;

    [ObservableProperty]
    private bool _isSwizzle;

    [ObservableProperty]
    private bool _isDeswizzle;

    [ObservableProperty]
    private Platform _platformType = Platform.None;

    [ObservableProperty]
    private bool _enableAlpha;

    [ObservableProperty]
    private T3TextureAlphaMode _alphaFormat;

    //[ObservableProperty]
    //private ImageEffect _imageEffect;

    // Store a reference to MainViewModel
    private readonly MainViewModel _mainViewModel;

    public ImageAdvancedOptions(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    public ImageAdvancedOptions(ImageAdvancedOptions imageAdvancedOptions)
    {
        _gameID = imageAdvancedOptions._gameID;
        _textureType = imageAdvancedOptions._textureType;
        _enableMips = imageAdvancedOptions._enableMips;
        _autoGenerateMips = imageAdvancedOptions._autoGenerateMips;
        _manualGenerateMips = imageAdvancedOptions._manualGenerateMips;
        _setMips = imageAdvancedOptions._setMips;
        _compression = imageAdvancedOptions._compression;
        _enableAutomaticCompression = imageAdvancedOptions._enableAutomaticCompression;
        _isAutomaticCompression = imageAdvancedOptions._isAutomaticCompression;
        _format = imageAdvancedOptions._format;
        _isTelltaleNormalMap = imageAdvancedOptions._isTelltaleNormalMap;
        _enableNormalMap = imageAdvancedOptions._enableNormalMap;
        _enableTelltaleNormalMap = imageAdvancedOptions._enableTelltaleNormalMap;
        _encodeDDSHeader = imageAdvancedOptions._encodeDDSHeader;
        _filterValues = imageAdvancedOptions._filterValues;
        _enableWrapU = imageAdvancedOptions._enableWrapU;
        _enableWrapV = imageAdvancedOptions._enableWrapV;
        _enableEditing = imageAdvancedOptions._enableEditing;
        _enableSwizzle = imageAdvancedOptions._enableSwizzle;
        _isSwizzle = imageAdvancedOptions._isSwizzle;
        _isDeswizzle = imageAdvancedOptions._isDeswizzle;
        _platformType = imageAdvancedOptions._platformType;
        _enableAlpha = imageAdvancedOptions._enableAlpha;
        _alphaFormat = imageAdvancedOptions._alphaFormat;
      //  _imageEffect = imageAdvancedOptions._imageEffect;
        _mainViewModel = imageAdvancedOptions._mainViewModel;
    }

    public ImageAdvancedOptions() { }

    // Override OnPropertyChanged to trigger the MainViewModel's command
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // Trigger the command in MainViewModel
        _mainViewModel.UpdateBitmapCommand.Execute(null);
    }
}
