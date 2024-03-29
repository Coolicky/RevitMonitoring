﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Monitoring.Setup.Wpf
{
    public class ReverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && !(bool)value;
        }
    }

    public class ProgressPercentageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var divided = System.Convert.ToDouble((int)values[0]);
            var divider = (int)values[1] > 0 ? (int)values[1] : 1;
            var divide = divided / divider;
            var percentage = Math.Round(divide * 100, MidpointRounding.ToEven);
            if (percentage < 0) return 0;
            if (percentage > 100) return 100;
            return $"{percentage}%";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
    public class MultiBoolAndConverterToVisibility : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (bool value in values)
            {
                if (!value) return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}