using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TelltaleTextureTool.Codecs;
using TelltaleTextureTool.DirectX;
using TelltaleTextureTool.Graphics;
using TelltaleTextureTool.TelltaleEnums;
using TelltaleTextureTool.ViewModels;
using ImageEffect = TelltaleTextureTool.Graphics.ImageEffect;

namespace TelltaleTextureTool;

public partial class ConverterOptions : ObservableObject
{
    // Private telltale games
    [ObservableProperty]
    private TelltaleToolGame _gameID = TelltaleToolGame.NONE;

    [ObservableProperty]
    private TextureType _textureType;

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
    private bool _encodeDDSHeader;

    [ObservableProperty]
    private bool _filterValues;

    [ObservableProperty]
    private bool _enableWrapU;

    [ObservableProperty]
    private bool _enableWrapV;

    [ObservableProperty]
    private Platform _platformType = Platform.None;

    [ObservableProperty]
    private bool _enableAlpha;

    [ObservableProperty]
    private T3TextureAlphaMode _alphaFormat;

    [ObservableProperty]
    private ImageEffect _imageEffect;

    // Store a reference to MainViewModel
    private readonly MainViewModel _mainViewModel;

    public ConverterOptions(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    public ConverterOptions(ConverterOptions converterOptions)
    {
        _gameID = converterOptions._gameID;
        _textureType = converterOptions._textureType;
        _setMips = converterOptions._setMips;
        _compression = converterOptions._compression;
        _enableAutomaticCompression = converterOptions._enableAutomaticCompression;
        _isAutomaticCompression = converterOptions._isAutomaticCompression;
        _format = converterOptions._format;
        _isTelltaleNormalMap = converterOptions._isTelltaleNormalMap;

        _enableTelltaleNormalMap = converterOptions._enableTelltaleNormalMap;
        _encodeDDSHeader = converterOptions._encodeDDSHeader;
        _filterValues = converterOptions._filterValues;
        _enableWrapU = converterOptions._enableWrapU;
        _enableWrapV = converterOptions._enableWrapV;

        _platformType = converterOptions._platformType;
        _enableAlpha = converterOptions._enableAlpha;
        _alphaFormat = converterOptions._alphaFormat;
        //  _imageEffect = imageAdvancedOptions._imageEffect;
        _mainViewModel = converterOptions._mainViewModel;
    }

    public ConverterOptions() { }

    // Override OnPropertyChanged to trigger the MainViewModel's command
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // Trigger the command in MainViewModel
        _mainViewModel.UpdateBitmapCommand.Execute(null);
    }
}
