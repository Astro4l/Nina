﻿#region "copyright"

/*
    Copyright © 2016 - 2019 Stefan Berg <isbeorn86+NINA@googlemail.com>

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

using NINA.Utility.Astrometry;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NINA.Utility.SkySurvey {

    internal class FileSkySurvey : ISkySurvey {

        public async Task<SkySurveyImage> GetImage(string name, Coordinates coordinates, double fieldOfView, int width,
            int height, CancellationToken ct, IProgress<int> progress) {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = Locale.Loc.Instance["LblLoadImage"];
            dialog.FileName = "";
            dialog.DefaultExt = ".tif";
            dialog.Multiselect = false;
            dialog.Filter = "Image files|*.tif;*.tiff;*.jpeg;*.jpg;*.png;*.cr2;*.nef|TIFF files|*.tif;*.tiff;|JPEG files|*.jpeg;*.jpg|PNG Files|*.png|RAW Files|*.cr2;*.nef";

            if (dialog.ShowDialog() == true) {
                BitmapSource img = null;
                switch (Path.GetExtension(dialog.FileName).ToLower()) {
                    case ".tif":
                    case ".tiff":
                        img = LoadTiff(dialog.FileName);
                        break;

                    case ".png":
                        img = LoadPng(dialog.FileName);
                        break;

                    case ".jpg":
                        img = LoadJpg(dialog.FileName);
                        break;

                    case ".cr2":
                    case ".nef":
                        img = await LoadRAW(dialog.FileName, ct);
                        break;
                }

                if (img == null) {
                    return null;
                }

                return new SkySurveyImage() {
                    Name = Path.GetFileNameWithoutExtension(dialog.FileName),
                    Coordinates = null,
                    FoVHeight = double.NaN,
                    FoVWidth = double.NaN,
                    Image = img,
                    Rotation = double.NaN,
                    Source = nameof(FileSkySurvey)
                };
            } else {
                return null;
            }
        }

        private BitmapSource LoadPng(string filename) {
            PngBitmapDecoder PngDec = new PngBitmapDecoder(new Uri(filename), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            return ConvertTo96Dpi(PngDec.Frames[0]);
        }

        private BitmapSource LoadJpg(string filename) {
            JpegBitmapDecoder JpgDec = new JpegBitmapDecoder(new Uri(filename), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            return ConvertTo96Dpi(JpgDec.Frames[0]);
        }

        private BitmapSource LoadTiff(string filename) {
            TiffBitmapDecoder TifDec = new TiffBitmapDecoder(new Uri(filename), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            return ConvertTo96Dpi(TifDec.Frames[0]);
        }

        public static BitmapSource ConvertTo96Dpi(BitmapSource image) {
            if (image.DpiX != 96) {
                byte[] pixelData = new byte[image.PixelWidth * 4 * image.PixelHeight];
                image.CopyPixels(pixelData, image.PixelWidth * 4, 0);

                return BitmapSource.Create(image.PixelWidth, image.PixelHeight, 96, 96, image.Format, null, pixelData,
                    image.PixelWidth * 4);
            }

            return image;
        }

        private async Task<BitmapSource> LoadRAW(string filename, CancellationToken ct) {
            using (FileStream fs = new FileStream(filename, FileMode.Open)) {
                using (MemoryStream ms = new MemoryStream()) {
                    fs.CopyTo(ms);
                    var converter = RawConverter.RawConverter.CreateInstance(Enum.RawConverterEnum.DCRAW);
                    var iarr = await converter.ConvertToImageArray(ms, 16, 0, false, ct);
                    return ImageAnalysis.CreateSourceFromArray(iarr, System.Windows.Media.PixelFormats.Gray16);
                }
            }
        }
    }
}