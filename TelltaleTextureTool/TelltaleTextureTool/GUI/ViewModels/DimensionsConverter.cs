using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;

namespace TelltaleTextureTool.ViewModels
{
    public class DimensionsConverter : IMultiValueConverter
    {
        public object? Convert(
            IList<object?> values,
            Type targetType,
            object? parameter,
            CultureInfo culture
        )
        {
            if (
                values.Count >= 4
                && values[0] is string width
                && values[1] is string height
                && values[2] is string depth
                && values[3] is string dimension
            )
            {
                if (width == string.Empty || height == string.Empty || depth == string.Empty || dimension == string.Empty)
                {
                    return string.Empty;
                }

                return Int32.Parse(depth) == 1
                    ? $"{width} × {height} px ({dimension})"
                    : $"{width} × {height} × {depth} px ({dimension})";
            }
            return string.Empty;
        }
    }
}
