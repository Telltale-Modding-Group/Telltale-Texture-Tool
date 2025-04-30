using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using static TelltaleTextureTool.ViewModels.MainViewModel;

namespace TelltaleTextureTool.ViewModels;

public class BackgroundTypeToBrushConverter : IValueConverter
{
    private static readonly IBrush CheckerboardBrush = CreateCheckerboardBrush();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not BackgroundType backgroundType)
            return Brushes.Transparent;

        return backgroundType switch
        {
            BackgroundType.Transparent => Brushes.Transparent,
            BackgroundType.Checkerboard => CheckerboardBrush,
            BackgroundType.White => Brushes.White,
            BackgroundType.Black => Brushes.Black,
            _ => Brushes.Transparent,
        };
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }

    private static DrawingBrush CreateCheckerboardBrush(int squareSize = 16)
    {
        var tileSize = squareSize * 2;

        return new DrawingBrush
        {
            Drawing = new DrawingGroup
            {
                Children =
                {
                    // White background (full tile area)
                    new GeometryDrawing
                    {
                        Brush = Brushes.White,
                        Geometry = new RectangleGeometry(new Rect(0, 0, tileSize, tileSize)),
                    },
                    // Gray squares
                    new GeometryDrawing
                    {
                        Brush = Brushes.LightGray,
                        Geometry = new GeometryGroup
                        {
                            Children =
                            {
                                new RectangleGeometry(new Rect(0, 0, squareSize, squareSize)),
                                new RectangleGeometry(
                                    new Rect(squareSize, squareSize, squareSize, squareSize)
                                ),
                            },
                        },
                    },
                },
            },
            DestinationRect = new RelativeRect(
                new Rect(0, 0, tileSize, tileSize),
                RelativeUnit.Absolute
            ),
            TileMode = TileMode.Tile,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            Stretch = Stretch.None,
        };
    }
}
