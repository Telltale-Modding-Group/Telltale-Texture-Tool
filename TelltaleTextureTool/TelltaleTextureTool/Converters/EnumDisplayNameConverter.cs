using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Avalonia.Data.Converters;

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
        DisplayAttribute attribute = field
            ?.GetCustomAttributes(false)
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
                var attribute = field
                    .GetCustomAttributes(false)
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
