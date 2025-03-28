using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TelltaleTextureTool.ViewModels;

public partial class DataGridColumnVisibilitySettings : ObservableObject
{
    [ObservableProperty]
    public bool _isNameVisible = true;

    [ObservableProperty]
    public bool _isExtensionVisible = true;

    [ObservableProperty]
    public bool _isModifiedDateVisible = false;

    [ObservableProperty]
    public bool _isCreatedDateVisible = false;

    [ObservableProperty]
    public bool _isSizeVisible = false;
}

