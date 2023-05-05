﻿#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com>

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

namespace NINA.Profile {

    public interface IProfile : IDisposable, INotifyPropertyChanged {
        Guid Id { get; set; }
        string Name { get; set; }
        string Location { get; }
        DateTime LastUsed { get; }
        IApplicationSettings ApplicationSettings { get; set; }
        //zhang106
        //IAstrometrySettings AstrometrySettings { get; set; }
        ICameraSettings CameraSettings { get; set; }
        IColorSchemaSettings ColorSchemaSettings { get; set; }
        IFilterWheelSettings FilterWheelSettings { get; set; }
        //zhang093
        //IFlatWizardSettings FlatWizardSettings { get; set; }
        //IFocuserSettings FocuserSettings { get; set; }
        //IFramingAssistantSettings FramingAssistantSettings { get; set; }
        //IGuiderSettings GuiderSettings { get; set; }
        IImageFileSettings ImageFileSettings { get; set; }
        IImageSettings ImageSettings { get; set; }
        //IMeridianFlipSettings MeridianFlipSettings { get; set; }
        //IPlanetariumSettings PlanetariumSettings { get; set; }
        //IPlateSolveSettings PlateSolveSettings { get; set; }
        //IPolarAlignmentSettings PolarAlignmentSettings { get; set; }
        //IRotatorSettings RotatorSettings { get; set; }
        //IFlatDeviceSettings FlatDeviceSettings { get; set; }
        ISequenceSettings SequenceSettings { get; set; }
        //ISwitchSettings SwitchSettings { get; set; }
        //ITelescopeSettings TelescopeSettings { get; set; }
        //IWeatherDataSettings WeatherDataSettings { get; set; }
        //IExposureCalculatorSettings ExposureCalculatorSettings { get; set; }
        ISnapShotControlSettings SnapShotControlSettings { get; set; }

        void Save();
    }
}