using System.Globalization;
using Avalonia.Data.Converters;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.Helpers;

public class IsLocalTestAvatarConverter : IValueConverter
{
    public static readonly IsLocalTestAvatarConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string id && id.StartsWith("local:");

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class LegacyParameterCountConverter : IValueConverter
{
    public static readonly LegacyParameterCountConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is List<Parameter> parameters ? parameters.Count(p => p.Deprecated) : 0;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class HasLegacyParametersConverter : IValueConverter
{
    public static readonly HasLegacyParametersConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is List<Parameter> parameters && parameters.Any(p => p.Deprecated);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
