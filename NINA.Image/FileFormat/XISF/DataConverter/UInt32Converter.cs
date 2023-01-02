#region "copyright"

/*
    Copyright � 2016 - 2023 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

namespace NINA.Image.FileFormat.XISF.DataConverter {

    internal class UInt32Converter : IDataConverter {

        public ushort[] Convert(byte[] rawData) {
            ushort[] data = new ushort[rawData.Length / 4];
            for (var i = 0; i < data.Length; i++) {
                data[i] = (ushort)(((rawData[(i * 4) + 3] << 24) | (rawData[(i * 4) + 2] << 16) | (rawData[(i * 4) + 1] << 8) | (rawData[i * 4])) / (double)int.MaxValue * ushort.MaxValue);
            }
            return data;
        }
    }
}