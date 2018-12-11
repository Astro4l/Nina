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

using System;
using System.ComponentModel;

namespace NINA.Utility.Profile {

    public interface IProfile : INotifyPropertyChanged {
        IApplicationSettings ApplicationSettings { get; set; }
        IAstrometrySettings AstrometrySettings { get; set; }
        ICameraSettings CameraSettings { get; set; }
        IColorSchemaSettings ColorSchemaSettings { get; set; }
        IFilterWheelSettings FilterWheelSettings { get; set; }
        IFocuserSettings FocuserSettings { get; set; }
        IFramingAssistantSettings FramingAssistantSettings { get; set; }
        IGuiderSettings GuiderSettings { get; set; }
        Guid Id { get; set; }
        IImageFileSettings ImageFileSettings { get; set; }
        IImageSettings ImageSettings { get; set; }
        bool IsActive { get; set; }
        IMeridianFlipSettings MeridianFlipSettings { get; set; }
        string Name { get; set; }
        IPlateSolveSettings PlateSolveSettings { get; set; }
        IPolarAlignmentSettings PolarAlignmentSettings { get; set; }
        IRotatorSettings RotatorSettings { get; set; }
        ISequenceSettings SequenceSettings { get; set; }
        ITelescopeSettings TelescopeSettings { get; set; }
        IWeatherDataSettings WeatherDataSettings { get; set; }

        void MatchFilterSettingsWithFilterList();
    }
}