### Version 2.5.1

- Added support for custom Monkey Island textures (Thanks HeitorSpectre!)
- Fixed `Puzzle Agent 2`, `Jurassic Park` and `Law & Order Legacies` writing. (Fixes #15, Thanks @webfischi!)
- Fixed `Texas Hold'Em` and `Sam & Max Beyond Time and Space` reading and writing.
- Added tooltips for the data grid. (Thanks @WeaselOnaStick!)
- Removed multi-selection from the data grid. (Finally! Thanks @WeaselOnaStick!)
- Improved `Debug Information` text.
- Fixed meta class names not displaying.
- Bumped all Avalonia dependencies to 11.2.0.
- Utilized StringBuilder to improve string performance.

### Version 2.5.0

### Main Window
- New top-bar menu
  - Moved the old top left buttons
  - Merged the help and about button in one menu button
- There are now 3 resizeable columns.
   - Left column is the standard file explorer with some minor tweaks. Additionally, the `Date Modified` column field is minimized by default. You can resize it.
   - Middle column is the Image Preview with mip and face sliders.
   - Right column contains expanders about image properties, advanced editing options, debug output and conversion menu.

### Image Preview
- Added `Pan and Zoom` feature. Use the scroll wheel to zoom in/out and the left mouse button to drag the image.
- Added mip and face sliders. They automatically hide if there are no mips or faces presented, respectively. Very useful for cubemaps, arraymaps and volumemaps.

### Right Column

#### Image Properties
- A couple of new fields were added, most notably texture layout and colour space.
- Removed channel count field. It did not always work. For compressed formats, it would be better to search them up in the documentation.
- `Has Alpha` has been renamed to `Has Transparency`.

#### Advanced Options
It contains:
- Combobox containing all games before and including The Walking Dead Season 1. It is used for legacy formats only.
- `Legacy Console` -  if checked, older console textures can be read. The actual texture may be not be read.
- `Auto Compression` - if checked, the converter picks the suitable compression method based on 
- `Auto Normal Maps` - if checked, the converter picks the suitable compression and effects to make normal maps work for Telltale Tool Engine.
- `Enable Mipmaps` - if checked, there will be 2 options: automatic and manual.
   - Automatic will generate the appropriate amount of mips
   - Manual - you can select the number of mips that will be generated.
- `Enable Swizzling` - if checked, there will be a combobox containing different platforms with 2 radio buttons - Swizzle and Deswizzle. These are used for textures from different platforms like PS3, PS4, Xbox 360 and Nintendo Switch.
- `Enable Effects` - if checked, there will be a combobox containing some effects, useful for normal maps.
- `Create Normal Map` - if checked, the texture will be converted to a normal map. Note: The quality is not good at the moment - it might be tweaked in the future.

> [!WARNING] 
> These options apply only in the conversion process. They are not applied to the image itself!

#### Debug Information
- The contents from `Debug Info` button has been moved there. It makes it more consistent and more accessible.

#### Conversion Panel
- The `Conversion Options` has been split into 2 parts - `From` and `To`. It's more flexible and makes bulk conversion easier and more accessible.

### Converter:
- Added Linux and (potentially) MacOS support.
- Added support for reading and writing for every single Telltale game.
- Added support for reading and writing console textures (PS3, PS4, Nintendo Switch, Xbox 360).
- Added support for reading mobile textures for newer games (iOS and Android).
- Added HDR and TGA support.
- Added support for direct conversion from D3DTX to PNG, JPEG, TGA, HDR, BMP and TIFF, and vice versa.
- Added a new message box which indicates if a bulk conversion was successful or not. (Thanks [Mawrak](https://github.com/Mawrak)!)
- JSON files now contain `GameID` and `PlatformType` fields. `ConversionType` field is no longer used, but left out for backwards compatibility. v2.3.0 JSON files are no longer supported, v2.4.0 JSON files could potentially break in some cases. Please report any issues!
- Removed the option from other file formats to DDS, the advanced options replace that functionality.

### Bugfixes:
> [!WARNING] 
> Because a lot of the UI has changed, bugs are expected.

### Technical Improvements:
- Replaced DirectXTexNet with [Hexa.NET.DirectXTex](https://github.com/HexaEngine/Hexa.NET.DirectXTex). (Thanks [Juna Meinhold](https://github.com/JunaMeinhold), main developer of [Hexa Engine](https://github.com/HexaEngine/HexaEngine)!)
- Removed Texconv dependency.
- Added DrSwizzler library, allowing support for console platforms.
- Added BCnEncoder.Net, allowing support for Android.
- Reorganized the whole project for better readability.
- Refactored a lot of classes and functions.
- Removed or archived a lot of unused code and files. The project should be a lot cleaner with less code. 
- Bumped all dependencies to their latest versions.
- Removed ImageSharp, Pfim and LibTiff.Net libraries. They are replaced by SkiaSharp and DirectXTex/Hexa.NET.DirectxXTex.

### Version 2.4.0

#### Wiki (WIP):
- Initial commit for the wiki. It is serviceable, but not yet fully completed.
- Contains a basic tutorial applicable to other games.
- Contains additional articles for textures, archives and surface formats.

#### Converter:
- Added Poker Night 2 (2013) support (mVersion 3 games).
- Added Minecraft Story Mode: Season One - Xbox One support (mVersion 6 games). (Thanks [Knollad Knolladious](https://www.youtube.com/channel/UCegvS4IJnO926qnuIEfQfJw)!)
- Added TWD: Michonne (2016) support (mVersion 7 games). 
- Added support for some legacy Telltale games (pre-Poker Knight 2). These include:
    - The Walking Dead (2012) (Thanks [Lucas Saragosa](https://github.com/LucasSaragosa)!)
    - Jurassic Park (2011)
    - Back to the Future: The game (2010)
    - Puzzle Agent 2 (Untested)
    - Law & Order: Legacies (Untested)
- Added array texture support (Thanks [Knollad Knolladious](https://www.youtube.com/channel/UCegvS4IJnO926qnuIEfQfJw)!).
- Added cubemap texture support. Previously they were split into 6 separate DDS images.
- Added cubemap array texture support (They usually do not exist. Please report if you find any).
- Added volumemap texture support (They usually do not exist. Please report if you find any).
- Added support to export a DDS image even if the version is not recognized (it will not generate a JSON file). This is a feature parity with Telltale Explorer and should work for any Telltale game.
- Reworked the JSON file - it now has an additional field class that includes the conversion type used. The app is still compatible with older versions.
- Added the Debug CLI back, which will act as the main one for now.
- Improved the bulk conversion performance, it now converts more textures per thread.

#### GUI: 
- Added a Conversion Type combobox. 
- Added support for previewing older D3DTX files. It should be almost up-to-par to that of Telltale Explorer. 
- Added a Debug info button. When clicked on, it will display in a dialog the information about DDS images and D3DTX files.
- Added a new exporting feature, when the mVersion is unknown. It is used when the "Default" option is used.
- Changed the names of some Telltale surface formats. They are now shown a little bit more consistent with their more known DXGI counterparts.
    - DXT1   now shows as BC1
    - DXT3   now shows as BC2
    - DXT5   now shows as BC3
    - DXT5a now shows as BC4
    - DXN    now shows as BC5
- Reorganized some elements. 
    - Image preview should be a little bigger than before.
    - Image properties take the whole panel space.
    - The data grid is now expanded to the bottom.
- Improved "Add Files" button.
- Renamed the app title to "Telltale Texture Mod Tool".
- Updated the Help button link.
- Updated the About window.
- Enhanced some functions.
- Improved window and column resizing.

#### Bugfixes:
The converter is now more robust than before, which fixes a lot of bugs.
- Fixed a lot of bugs regarding DDS header writing including flags, mips and pixel formats. 
    - Fixed Telltale A8 surface format conversion.
    - Fixed potentially ARGB2101010.
    - Fixed sRGB conversion.
    - Fixed other pixel formats headers.
    - Fixed extracting the data correctly.
    - Fixed an inconvenience where if the texture had no mips it still had it enabled in the header.
- Fixed incorrect Telltale surface formats enum values.
- Fixed writing non-existing compression formats.
- Fixed a lot of DDS reading bugs.
- Fixed all region indexing when writing to D3DTX.
- Fixed SurfaceGamma not changing depending on the DDS format.
- Fixed channel count displaying wrong values in image properties (partially).
- Fixed transparency issue DDS images. Previously transparent pixels appeared white. Now, a side effect is slower load time (but worth it).
- Fixed displaying rare surface formats which include but are not limited to A8, L8, A8L8, L16, R16, RGBA16, ARGB16 and more.
- Fixed SamplerState not having correct values.
- Fixed not reading images with uppercase extensions.
- Fixed bulk conversion when the output directory was not set.
- Fixed "Delete File" from context menu not working.
- Fixed error message notification when converting from DDS to other file formats, even thought it is created.
- Fixed error message notification that when converting from other file formats to DDS the JSON was not found.
- Fixed GUI resizing of some elements.
- Fixed GUI window resizing not working properly.
- Fixed "Refresh Button" not working properly.
- Fixed the context menu "Refresh" command not working properly.
- Fixed many other minor bugs.

#### Technical Improvements:
- The project now fully utilizes the DirectXTex .NET wrapper. This brings a lot of bug fixes, overall improvements and readability to the codespace.
- Reorganized the whole project for better readability.
- Refactored a lot of classes and functions.
- Removed or archived a lot of unused code and files. The project should be a lot cleaner with less code. 
- Reduced tool size (with around 15MBs).
- Deprecated D3DTX_V8.
- Bumped DirectXTexNet to v1.0.7 (it now uses DirectXTex March 2024 version).
- Bumped Avalonia.Xaml.Behaviors to v11.0.10.9.
- Bumped Avalonia.Svg.Skia to v11.0.0.18.
- Bumped ImageSharp to v3.1.4.

### Version 2.3.0

#### Changelog:
- Updated the README to be more up-to-date, which now includes an actual screenshot of the new converter
- Added bulk conversion. The simplest way to use it: select a folder, choose the desired option and convert
- Added a checkbox for optional output directory
- Many bug fixes to DDS -> D3DTX conversion process. DDS files now finally convert correctly
- D3DTX meta headers now convert properly
- Scrollbar resetting after conversion is now fixed, but there is a GUI small side effect with focusing on the selectable item
- Some small GUI QOL improvements

### Version 2.2.0

#### Changelog:
- D3DTX_V5 - D3DTX_V8 files now should save properly when converting from .dds
- Some fixes related to surface formats
- Some fixes related to D3DTX -> DDS conversion
- Added initial header file for The Walking Dead
- File items in the app explorer are now filtered - only relevant files will show (.d3dtx, .dds, .json and supported image formats)

### Version 2.1.0

#### Changelog:
- Added more surface format support.
- Fixed conversion with other compression formats.
- Fixed mToolProps - it now saves correctly in the .json files.
- Fixed EnvMap files saving destination paths and .json files.

### Version 2.0.1 Hotfix

#### Changelog:
- Fixed converting to other file formats when path contains whitespaces.
- Fixed previewing other d3dtx formats.

### Version 2.0.0 (New GUI)

#### Changelog:
- Initial commit of the new GUI converter

#### Features:
- Converts the following formats: .d3dtx <-> .dds, .dds <-> .png, .jpg, .bmp, .tiff.
- Preview the images and its properties (if supported).
- The GUI acts like a file explorer. You are able to open and delete folders and files using double clicking, the menu buttons or context menu buttons.

### Version CLI Beta 2.0.1
- Fixed missing framework issue.

### Version CLI Beta Pre-release

- Fully support D3DTX conversions without restrictions with a functioning CLI.

### Version 1.0.0 (Original GUI)

#### Changelog
- First Release



