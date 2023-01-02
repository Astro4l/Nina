﻿#region "copyright"

/*
    Copyright © 2016 - 2023 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace NINA.Core.Utility.Converters {

    public class IntListToTextBlockListConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return new List<int>();
            return ((List<int>)value).Select(r => new TextBlock() { Text = r.ToString() });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            List<int> returnList = new List<int>();
            foreach (var item in (List<TextBlock>)value) {
                if (int.TryParse(item.Text.ToString(), out int result)) {
                    returnList.Add(result);
                }
            }
            return returnList;
        }
    }
}