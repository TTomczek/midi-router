using System.Globalization;
using System.Windows.Data;

namespace midi_router;

public sealed class QuarterScreenHeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double screenHeight)
        {
            throw new ArgumentException("The value must be a screen height.", nameof(value));
        }

        return screenHeight * 0.25;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
