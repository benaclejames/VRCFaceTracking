using Microsoft.UI.Xaml.Data;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Helpers;

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is InstallState state)
            if (state == InstallState.NotInstalled)
                return "";
        return $"({value})";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}