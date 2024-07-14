using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace VRCFaceTracking.Helpers;
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if ( value.GetType() == typeof(bool) )
        {
            if ( (string)parameter == "Inverse" )
            {
                return ( bool )value == true ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return ( bool )value == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
