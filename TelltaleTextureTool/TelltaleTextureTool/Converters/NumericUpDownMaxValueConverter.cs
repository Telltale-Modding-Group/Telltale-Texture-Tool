using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace TelltaleTextureTool.ViewModels;

public class MaxValueDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (
            value is decimal numericValue
            && parameter is string maxValueStr
            && decimal.TryParse(maxValueStr, out decimal maxValue)
        )
        {
            return numericValue >= maxValue ? "MAX" : value.ToString();
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (
            value is string strValue
            && strValue == "MAX"
            && parameter is string maxValueStr
            && decimal.TryParse(maxValueStr, out decimal maxValue)
        )
        {
            return maxValue;
        }

        if (decimal.TryParse(value?.ToString(), out decimal result))
        {
            return result;
        }

        return 0m; // Default value if conversion fails
    }
}
