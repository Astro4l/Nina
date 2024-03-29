#region "copyright"

/*
    Copyright © 2016 - 2023 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Astrometry;
using System;

namespace NINA.PlateSolving {

    public class PlateSolveResult {

        public PlateSolveResult() {
            Success = true;
            SolveTime = DateTime.Now;
        }

        public DateTime SolveTime { get; private set; }

        [Obsolete("Use PositionAngle instead")]
        public double Orientation {
            get => AstroUtil.EuclidianModulus(360 - PositionAngle, 360);
            set => PositionAngle = 360 - value;
        }

        private double positionAngle;
        public double PositionAngle {

            get => positionAngle;
            set => positionAngle = AstroUtil.EuclidianModulus(value, 360);
        }

        public double Pixscale { get; set; }

        public double Radius { get; set; }

        private Coordinates coordinates;

        public Coordinates Coordinates {
            get => coordinates;
            set => coordinates = value?.Transform(Epoch.J2000);
        }

        public bool Flipped { get; set; }

        public bool Success { get; set; }

        public Separation Separation { get; set; }

        public string RaErrorString => Separation == null ? "--" : AstroUtil.DegreesToHMS(Separation.RA.Degree);

        public double RaPixError => Separation == null ? double.NaN : Math.Round(Separation.RA.ArcSeconds / Pixscale, 2);

        public double DecPixError => Separation == null ? double.NaN : Math.Round(Separation.Dec.ArcSeconds / Pixscale, 2);

        public string DecErrorString => Separation == null ? "--" : AstroUtil.DegreesToDMS(Separation.Dec.Degree);
    }
}