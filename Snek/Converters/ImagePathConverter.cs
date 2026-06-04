using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Snek.Converters;

public class ImagePathConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var path = value as string;

        if (string.IsNullOrWhiteSpace(path))
            return LoadFromResources("picture.png");

        // Если путь начинается с "UserImages/" — это загруженная админом фотка на диске.
        if (path.StartsWith("UserImages", StringComparison.OrdinalIgnoreCase))
        {
            var full = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            if (File.Exists(full))
                return LoadFromDisk(full);
            return LoadFromResources("picture.png");
        }
        
        if (Path.IsPathRooted(path) && File.Exists(path))
            return LoadFromDisk(path);

        // Иначе пробуем как ресурсную картинку.
        return LoadFromResources(path) ?? LoadFromResources("picture.png");
    }

    private static BitmapImage? LoadFromResources(string fileName)
    {
        try
        {
            var uri = new Uri($"pack://application:,,,/Resources/Images/{fileName}",
                UriKind.Absolute);
            return CreateBitmap(uri);
        }
        catch
        {
            return null;
        }
    }

    private static BitmapImage LoadFromDisk(string fullPath)
    {
        return CreateBitmap(new Uri(fullPath, UriKind.Absolute));
    }

    private static BitmapImage CreateBitmap(Uri uri)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.UriSource = uri;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}