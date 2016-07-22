using System;
using System.Windows.Data;

namespace Hameg8118
{
    /// <summary>
    /// Converter that makes an inverse of a boolean value
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }                
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }
            return !(bool)value;            
        }
    }

    /// <summary>
    /// Converter for displaying values with scientific prefixes
    /// </summary>
    class Converters
    {        
        /// <summary>
        /// Method that converts a raw value to value with scientific prefix
        /// </summary>
        /// <param name="value">Value to scale, passed by reference</param>
        /// <returns>Scientific prefix</returns>
        public static string Prefix(ref double value)
        {
            if (Math.Abs(value) >= 1e12)
            {
                value /= 1e12;
                return "T";
            }
            if (Math.Abs(value) >= 1e9)
            {
                value /= 1e9;
                return "G";
            }
            if (Math.Abs(value) >= 1e6)
            {
                value /= 1e6;
                return "M";
            }
            if (Math.Abs(value) >= 1e3)
            {
                value /= 1e3;
                return "k";
            }

            if (Math.Abs(value) >= 1)
            {
                return string.Empty;
            }

            if (Math.Abs(value) >= 1e-3)
            {
                value /= 1e-3;
                return "m";
            }
            if (Math.Abs(value) >= 1e-6)
            {
                value /= 1e-6;
                return "µ";
            }
            if (Math.Abs(value) >= 1e-9)
            {
                value /= 1e-9;
                return "n";
            }
            else
            {
                value /= 1e-12;
                return "p";
            }            
        }
    }
}
