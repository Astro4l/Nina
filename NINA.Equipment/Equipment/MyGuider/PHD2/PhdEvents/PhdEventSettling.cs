﻿#region "copyright"

/*
    Copyright © 2016 - 2023 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;

namespace NINA.Equipment.Equipment.MyGuider.PHD2.PhdEvents {

    public class PhdEventSettling : PhdEvent {

        [JsonProperty]
        public double Distance { get; set; }

        [JsonProperty]
        public int Time { get; set; }

        [JsonProperty]
        public double SettleTime { get; set; }
    }
}