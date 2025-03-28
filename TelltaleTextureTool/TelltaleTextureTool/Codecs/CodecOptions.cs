using TelltaleTextureTool.TelltaleEnums;

namespace TelltaleTextureTool.Codecs;

public partial class CodecOptions
{
    public TelltaleToolGame TelltaleToolGame { get; set; } = TelltaleToolGame.DEFAULT;
    public bool IsLegacyConsole { get; set; }
    public Platform UnswizzleMode { get; set; }
    public bool DecompressOnLoad { get; set; }

    public bool AutoTelltaleNormalMap { get; set; }
    public bool AutoCompress { get; set; }
    public bool ForceDx9Legacy { get; set; }
    public bool GenerateJSON { get; set; }
    public byte[] JSONData { get; set; }
}

public enum Platform
{
    None,
    WiiU,
    Switch,
    PS3,
    PS4,
    XboxOne,
    PS5,
    XboxX,
    Xbox360,
    PSVita,
}
