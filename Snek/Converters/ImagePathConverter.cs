using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Snek.Converters;

public class ImagePathConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var filename = value as string;

        if (string.IsNullOrEmpty(filename))
        {
            return LoadFromResources("picture.png");
        }
        
        var img = LoadFromResources(filename);
        return img ?? LoadFromResources("picture.png");
    }

    private static BitmapImage? LoadFromResources(string filename)
    {
        try
        {
            var uri = new Uri($"pack://application:,,,/Resources/{filename}", UriKind.Absolute);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = uri;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}