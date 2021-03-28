﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using NINA.Model;
using NINA.Profile;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Utility;
using NINA.Utility;
using NINA.Astrometry;
using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using NINA.Core.Enum;

namespace NINA.Sequencer.Conditions {

    [ExportMetadata("Name", "Lbl_SequenceCondition_AltitudeCondition_Name")]
    [ExportMetadata("Description", "Lbl_SequenceCondition_AltitudeCondition_Description")]
    [ExportMetadata("Icon", "WaitForAltitudeSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Condition")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class AltitudeCondition : SequenceCondition {
        private readonly IProfileService profileService;
        private InputCoordinates coordinates;
        private bool isEastOfMeridian;
        private double altitude;
        private double currentAltitude;
        private string risingSettingDisplay;
        private bool hasDsoParent;

        [ImportingConstructor]
        public AltitudeCondition(IProfileService profileService) {
            this.profileService = profileService;
            Coordinates = new InputCoordinates();
            Altitude = 30;

            //todo: find a better home for this
            Task.Run(async () => {
                await Task.Delay(15000);
                while (true) {
                    try {
                        CalculateCurrentAltitude();
                    } catch (Exception ee) {
                        Logger.Error(ee);
                    }
                    await Task.Delay(5000);
                }
            });
        }

        [JsonProperty]
        public InputCoordinates Coordinates {
            get => coordinates;
            set {
                if (coordinates != null) {
                    coordinates.PropertyChanged -= Coordinates_PropertyChanged;
                }
                coordinates = value;
                if (coordinates != null) {
                    coordinates.PropertyChanged += Coordinates_PropertyChanged;
                }
                RaisePropertyChanged();
                CalculateCurrentAltitude();
            }
        }

        private void Coordinates_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            CalculateCurrentAltitude();
        }

        [JsonProperty]
        public bool HasDsoParent {
            get => hasDsoParent;
            set {
                hasDsoParent = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public double Altitude {
            get => altitude;
            set {
                altitude = value;
                RaisePropertyChanged();
            }
        }

        public double CurrentAltitude {
            get => currentAltitude;
            private set {
                currentAltitude = value;
                RaisePropertyChanged();
            }
        }

        public bool IsEastOfMeridian {
            get => isEastOfMeridian;
            private set {
                isEastOfMeridian = value;
                RaisePropertyChanged();
            }
        }

        public string RisingSettingDisplay {
            get => risingSettingDisplay;
            private set {
                risingSettingDisplay = value;
                RaisePropertyChanged();
            }
        }

        public override object Clone() {
            return new AltitudeCondition(profileService) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description,
                Altitude = Altitude,
                Coordinates = Coordinates.Clone()
            };
        }

        public override void AfterParentChanged() {
            var coordinates = ItemUtility.RetrieveContextCoordinates(this.Parent).Item1;
            if (coordinates != null) {
                Coordinates.Coordinates = coordinates;
                HasDsoParent = true;
            } else {
                HasDsoParent = false;
            }
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(AltitudeCondition)}, Altitude >= {Altitude}";
        }

        public override void ResetProgress() {
        }

        public override void SequenceBlockFinished() {
        }

        public override void SequenceBlockStarted() {
        }

        public override bool Check(ISequenceItem nextItem) {
            CalculateCurrentAltitude();

            return IsEastOfMeridian || CurrentAltitude >= Altitude;
        }

        private void CalculateCurrentAltitude() {
            var location = profileService.ActiveProfile.AstrometrySettings;
            var altaz = Coordinates
                .Coordinates
                .Transform(
                    Angle.ByDegree(location.Latitude),
                    Angle.ByDegree(location.Longitude));
            IsEastOfMeridian = altaz.AltitudeSite == AltitudeSite.EAST;
            var currentAlt = Math.Round(altaz.Altitude.Degree, 2);
            CurrentAltitude = currentAlt;
            RisingSettingDisplay =
                IsEastOfMeridian ? "^" : "v";
        }
    }
}