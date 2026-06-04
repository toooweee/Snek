using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Snek.Converters;

public class ProductBackgroundConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return Brushes.Transparent;

        var stock = values[0] is int s ? s : 0;
        var discount = values[1] is int d ? d : 0;

        if (stock == 0)
            return new SolidColorBrush(Color.FromRgb(173, 216, 230));

        if (discount > 15)
            return (SolidColorBrush)new BrushConverter().ConvertFromString("#2E8B57")!;

        return Brushes.Transparent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}