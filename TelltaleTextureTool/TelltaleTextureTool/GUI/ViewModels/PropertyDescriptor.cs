using CommunityToolkit.Mvvm.ComponentModel;

namespace TelltaleTextureTool.GUI.ViewModels;

public partial class PropertyDescriptor : ObservableObject
{
    private readonly ImageProperties _source;

    public string DisplayName { get; }
    public string Description { get; }
    public string PropertyName { get; }

    [ObservableProperty]
    private string _value;

    // public PropertyDescriptor(string displayName, string propertyName, string description)
    // {
    //     _source = MainViewModel.ImageProperties; // Reference your actual source
    //     DisplayName = displayName;
    //     PropertyName = propertyName;
    //     Description = description;
    //     UpdateValue();
    // }

    public void UpdateValue()
    {
        var prop = _source.GetType().GetProperty(PropertyName);
        Value = prop?.GetValue(_source)?.ToString() ?? string.Empty;
    }
}
