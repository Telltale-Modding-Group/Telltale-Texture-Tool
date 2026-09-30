using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TelltaleTextureTool.ViewModels;

public partial class ImagePropertiesViewModel : ObservableObject
{
    // Definition of PropertyItem (should be in the same namespace)
    public partial class PropertyItem : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _value;
        public string ToolTip { get; }

        public PropertyItem(string name, string value, string toolTip) =>
            (Name, Value, ToolTip) = (name, value, toolTip);
    }

    // TreeDataGrid Source
    public FlatTreeDataGridSource<PropertyItem> Source { get; set; }
    public ObservableCollection<PropertyItem> Properties =
    [
        new("Name", string.Empty, "The name of the texture file"),
        new(
            "Dimensions",
            string.Empty,
            "Pixel dimensions (Width × Height x Depth). Includes texture layout information."
        ),
        new("Pixel Format", string.Empty, "Compression format (e.g., BC1, RGBA8)"),
        new("Mip Count", string.Empty, "Number of mipmap levels"),
        new("Array Size", string.Empty, "Number of textures in an array"),
        new("Alpha Mode", string.Empty, "Alpha channel mode (e.g., Premultiplied)"),
        new("Is Cubemap", string.Empty, "Indicates if the texture is a cubemap"),
        new("Is Volumemap", string.Empty, "Indicates if the texture is a volumetric map"),
    ];

    public ImagePropertiesViewModel()
    {
        Source = new FlatTreeDataGridSource<PropertyItem>(Properties)
        {
            Columns =
            {
                new TextColumn<PropertyItem, string>(
                    "Name",
                    x => x.Name,
                    (item, value) => item.Name = value,
                    null,
                    new TextColumnOptions<PropertyItem>
                    {
                        BeginEditGestures = BeginEditGestures.None,
                    }
                ),
                new TemplateColumn<PropertyItem>(
                    "Value",
                    new FuncDataTemplate<PropertyItem>(
                        (_, _) =>
                            new SelectableTextBlock
                            {
                                [!SelectableTextBlock.TextProperty] = new Binding("Value"),
                                [!ToolTip.ContentProperty] = new Binding("ToolTip"),
                                VerticalAlignment = VerticalAlignment.Center,
                            }
                    ),
                    new FuncDataTemplate<PropertyItem>(
                        static (_, _) =>
                            new SelectableTextBlock
                            {
                                [!TextBlock.TextProperty] = new Binding("Value"),
                                [!ToolTip.ContentProperty] = new Binding("ToolTip"),
                                VerticalAlignment = VerticalAlignment.Center,
                            }
                    ),
                    null,
                    new TemplateColumnOptions<PropertyItem>
                    {
                        BeginEditGestures = BeginEditGestures.None,
                    }
                ),
            },
        };
    }
}
