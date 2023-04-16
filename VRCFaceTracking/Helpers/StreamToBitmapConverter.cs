using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace VRCFaceTracking.Helpers;

public class StreamToBitmapConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var bitmapImages = new List<BitmapImage>();
        if (value == null)
            return bitmapImages;
        
        var imageSources = (List<Stream>)value;
        foreach (var imageSource in imageSources)
        {
            var bitmapImage = new BitmapImage();
            imageSource.Seek(0, SeekOrigin.Begin);
            bitmapImage.SetSource(imageSource.AsRandomAccessStream());
            bitmapImages.Add(bitmapImage);
        }
        return bitmapImages;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}