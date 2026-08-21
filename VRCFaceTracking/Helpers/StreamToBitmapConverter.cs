using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace VRCFaceTracking.Helpers;

public class StreamToBitmapConverter : IValueConverter
{
    public static readonly StreamToBitmapConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Stream stream)
        {
            return null;
        }

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return new Bitmap(stream);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class HasModuleImagesConverter : IValueConverter
{
    public static readonly HasModuleImagesConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is List<Stream> { Count: > 0 };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}