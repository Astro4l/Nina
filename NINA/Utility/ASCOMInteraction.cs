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

using NINA.Model.MyCamera;
using NINA.Model.MyFilterWheel;
using NINA.Model.MyFocuser;
using NINA.Model.MyRotator;
using NINA.Model.MyTelescope;
using NINA.Utility.Profile;
using System;
using System.Collections.Generic;

namespace NINA.Utility {

    internal class ASCOMInteraction {

        public static List<ICamera> GetCameras(IProfileService profileService) {
            var l = new List<ICamera>();
            var ascomDevices = new ASCOM.Utilities.Profile();
            foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("Camera")) {
                try {
                    AscomCamera cam = new AscomCamera(device.Key, device.Value + " (ASCOM)", profileService);
                    Logger.Trace(string.Format("Adding {0}", cam.Name));
                    l.Add(cam);
                } catch (Exception) {
                    //only add cameras which are supported. e.g. x86 drivers will not work in x64
                }
            }
            return l;
        }

        public static List<ITelescope> GetTelescopes(IProfileService profileService) {
            var l = new List<ITelescope>();
            var ascomDevices = new ASCOM.Utilities.Profile();

            foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("Telescope")) {
                try {
                    AscomTelescope telescope = new AscomTelescope(device.Key, device.Value, profileService);
                    l.Add(telescope);
                } catch (Exception) {
                    //only add telescopes which are supported. e.g. x86 drivers will not work in x64
                }
            }
            return l;
        }

        public static List<IFilterWheel> GetFilterWheels(IProfileService profileService) {
            var l = new List<IFilterWheel>();
            var ascomDevices = new ASCOM.Utilities.Profile();

            foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("FilterWheel")) {
                try {
                    AscomFilterWheel fw = new AscomFilterWheel(device.Key, device.Value);
                    l.Add(fw);
                } catch (Exception) {
                    //only add filter wheels which are supported. e.g. x86 drivers will not work in x64
                }
            }
            return l;
        }

        public static List<IRotator> GetRotators(IProfileService profileService) {
            var l = new List<IRotator>();
            var ascomDevices = new ASCOM.Utilities.Profile();

            foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("Rotator")) {
                try {
                    AscomRotator rotator = new AscomRotator(device.Key, device.Value);
                    l.Add(rotator);
                } catch (Exception) {
                    //only add filter wheels which are supported. e.g. x86 drivers will not work in x64
                }
            }
            return l;
        }

        public static List<IFocuser> GetFocusers(IProfileService profileService) {
            var l = new List<IFocuser>();
            var ascomDevices = new ASCOM.Utilities.Profile();

            foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("Focuser")) {
                try {
                    AscomFocuser focuser = new AscomFocuser(device.Key, device.Value);
                    l.Add(focuser);
                } catch (Exception) {
                    //only add filter wheels which are supported. e.g. x86 drivers will not work in x64
                }
            }
            return l;
        }

        public static string GetVersion() {
            return "Version " + new ASCOM.Utilities.Util().PlatformVersion;
        }
    }
}