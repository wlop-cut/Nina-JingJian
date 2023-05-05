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

using NINA.Model.MyCamera;
using NINA.Utility.Enum;

namespace NINA.Profile {

    public interface ICameraSettings : ISettings {
        double BitDepth { get; set; }
        CameraBulbModeEnum BulbMode { get; set; }
        string Id { get; set; }
        double PixelSize { get; set; }
        RawConverterEnum RawConverter { get; set; }
        string SerialPort { get; set; }
        double MinFlatExposureTime { get; set; }
        double MaxFlatExposureTime { get; set; }
        string FileCameraFolder { get; set; }
        bool FileCameraUseBulbMode { get; set; }
        bool FileCameraIsBayered { get; set; }
        string FileCameraExtension { get; set; }
        BayerPatternEnum BayerPattern { get; set; }

        bool FLIEnableFloodFlush { get; set; }
        bool FLIEnableSnapshotFloodFlush { get; set; }
        double FLIFloodDuration { get; set; }
        uint FLIFlushCount { get; set; }
        BinningMode FLIFloodBin { get; set; }

        bool BitScaling { get; set; }
    }
}