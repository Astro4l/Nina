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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace NINA.Utility.SkySurvey {

    internal class CacheSkySurvey {
        private string framingAssistantCachePath;
        private string framingAssistantCachInfo;

        public CacheSkySurvey(string framingAssistantCachePath) {
            this.framingAssistantCachePath = framingAssistantCachePath;
            this.framingAssistantCachInfo = Path.Combine(framingAssistantCachePath, "CacheInfo.xml");
            Initialize();
        }

        private void Initialize() {
            if (!Directory.Exists(framingAssistantCachePath)) {
                Directory.CreateDirectory(framingAssistantCachePath);
            }

            if (!File.Exists(framingAssistantCachInfo)) {
                XElement info = new XElement("ImageCacheInfo");
                info.Save(framingAssistantCachInfo);
                Cache = info;
                return;
            } else {
                Cache = XElement.Load(framingAssistantCachInfo);

                /* Ensure Backwards compatibility with v1.6.0.2 */
                var elements = Cache.Elements("Image").Where(x => x.Attribute("Id") == null);
                foreach (var element in elements) {
                    element.Add(new XAttribute("Id", Guid.NewGuid()));
                }
                elements = Cache.Elements("Image").Where(x => x.Attribute("Source") == null);
                foreach (var element in elements) {
                    if (element.Attribute("Rotation").Value != "0") {
                        element.Add(new XAttribute("Source", nameof(FileSkySurvey)));
                    } else {
                        element.Add(new XAttribute("Source", nameof(NASASkySurvey)));
                    }
                }
            }
        }

        public void Clear() {
            System.IO.DirectoryInfo di = new DirectoryInfo(framingAssistantCachePath);

            foreach (FileInfo file in di.GetFiles()) {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories()) {
                dir.Delete(true);
            }
            Initialize();
        }

        public XElement SaveImageToCache(SkySurveyImage skySurveyImage) {
            try {
                var element =
                    Cache
                    .Elements("Image")
                    .Where(
                        x => x.Attribute("Id").Value == skySurveyImage.Id.ToString()
                    ).FirstOrDefault();
                if (element == null) {
                    element =
                    Cache
                    .Elements("Image")
                    .Where(
                        x =>
                            x.Attribute("RA").Value == skySurveyImage.Coordinates.RA.ToString("R", CultureInfo.InvariantCulture)
                            && x.Attribute("Dec").Value == skySurveyImage.Coordinates.Dec.ToString("R", CultureInfo.InvariantCulture)
                            && x.Attribute("FoVW").Value == skySurveyImage.FoVWidth.ToString("R", CultureInfo.InvariantCulture)
                            && x.Attribute("Source").Value == skySurveyImage.Source
                    ).FirstOrDefault();

                    if (element == null) {
                        if (!Directory.Exists(framingAssistantCachePath)) {
                            Directory.CreateDirectory(framingAssistantCachePath);
                        }

                        var imgFilePath = Path.Combine(framingAssistantCachePath, skySurveyImage.Name + ".jpg");

                        imgFilePath = Utility.GetUniqueFilePath(imgFilePath);
                        var name = Path.GetFileNameWithoutExtension(imgFilePath);

                        using (var fileStream = new FileStream(imgFilePath, FileMode.Create)) {
                            var encoder = new JpegBitmapEncoder();
                            encoder.QualityLevel = 70;
                            encoder.Frames.Add(BitmapFrame.Create(skySurveyImage.Image));
                            encoder.Save(fileStream);
                        }

                        XElement xml = new XElement("Image",
                            new XAttribute("Id", skySurveyImage.Id),
                            new XAttribute("RA", skySurveyImage.Coordinates.RA.ToString("R", CultureInfo.InvariantCulture)),
                            new XAttribute("Dec", skySurveyImage.Coordinates.Dec.ToString("R", CultureInfo.InvariantCulture)),
                            new XAttribute("Rotation", skySurveyImage.Rotation),
                            new XAttribute("FoVW", skySurveyImage.FoVWidth.ToString("R", CultureInfo.InvariantCulture)),
                            new XAttribute("FoVH", skySurveyImage.FoVHeight.ToString("R", CultureInfo.InvariantCulture)),
                            new XAttribute("FileName", Path.GetFileName(imgFilePath)),
                            new XAttribute("Source", skySurveyImage.Source),
                            new XAttribute("Name", name)
                        );

                        Cache.Add(xml);
                        Cache.Save(framingAssistantCachInfo);
                        return xml;
                    }
                }
                return element;
            } catch (Exception ex) {
                Logger.Error(ex);
                throw ex;
            }
            return null;
        }

        public XElement Cache { get; private set; }

        public object CulutureInfo { get; private set; }

        public Task<SkySurveyImage> GetImage(Guid id) {
            return Task.Run(() => {
                var element =
                    Cache
                    .Elements("Image")
                    .Where(
                        x => x.Attribute("Id").Value == id.ToString()
                    ).FirstOrDefault();
                if (element != null) {
                    return Load(element);
                } else {
                    return null;
                }
            });
        }

        private SkySurveyImage Load(XElement element) {
            var img = LoadJpg(element.Attribute("FileName").Value);
            Guid id = Guid.Parse(element.Attribute("Id").Value);
            var fovW = double.Parse(element.Attribute("FoVW").Value, CultureInfo.InvariantCulture);
            var fovH = double.Parse(element.Attribute("FoVH").Value, CultureInfo.InvariantCulture);
            var rotation = double.Parse(element.Attribute("Rotation").Value, CultureInfo.InvariantCulture);
            var ra = double.Parse(element.Attribute("RA").Value, CultureInfo.InvariantCulture);
            var dec = double.Parse(element.Attribute("Dec").Value, CultureInfo.InvariantCulture);
            var name = element.Attribute("Name").Value;
            var source = element.Attribute("Source")?.Value ?? string.Empty;

            img.Freeze();
            return new SkySurveyImage() {
                Id = id,
                Image = img,
                FoVHeight = fovH,
                FoVWidth = fovW,
                Coordinates = new Coordinates(ra, dec, Epoch.J2000, Coordinates.RAType.Hours),
                Name = name,
                Rotation = rotation
            };
        }

        private BitmapSource LoadJpg(string filename) {
            if (!Path.IsPathRooted(filename)) {
                filename = Path.Combine(framingAssistantCachePath, filename);
            }

            JpegBitmapDecoder JpgDec = new JpegBitmapDecoder(new Uri(filename), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

            return FileSkySurvey.ConvertTo96Dpi(JpgDec.Frames[0]);
        }
    }
}