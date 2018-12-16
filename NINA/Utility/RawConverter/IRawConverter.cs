﻿#region "copyright"

/*
    Copyright © 2016 - 2018 Stefan Berg <isbeorn86+NINA@googlemail.com>

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    N.I.N.A. is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    N.I.N.A. is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with N.I.N.A..  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion "copyright"

using NINA.Model.MyCamera;
using NINA.Utility.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Utility.RawConverter {

    internal class RawConverter {

        public static IRawConverter CreateInstance(RawConverterEnum converter) {
            switch (converter) {
                case Enum.RawConverterEnum.DCRAW:
                    return new DCRaw();

                case Enum.RawConverterEnum.FREEIMAGE:
                    return new FreeImageConverter();

                default:
                    return new DCRaw();
            }
        }
    }

    internal interface IRawConverter {

        Task<ImageArray> ConvertToImageArray(MemoryStream s, int bitDepth, int histogramResolution, CancellationToken token);
    }
}