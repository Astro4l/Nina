﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace NINA.Utility.Converters {

    internal class InverseMultiBooleanANDConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            for (int i = 0; i < values.Length; i++) {
                if (values[i] == DependencyProperty.UnsetValue) {
                    values[i] = false;
                }
            }

            return values.All(x => (bool)x == false);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}