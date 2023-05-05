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

namespace NINA.Model.MyCamera {

    public enum SensorType {
        /*
         * 0-19: ASCOM definitions
         */

        //     monochrome - no bayer encoding
        Monochrome = 0,

        //     Color image without bayer encoding
        Color = 1,

        //     RGGB bayer encoding
        RGGB = 2,

        //     CMYG bayer encoding
        CMYG = 3,

        //     CMYG2 bayer encoding
        CMYG2 = 4,

        //     Camera produces Kodak TRUESENSE Bayer LRGB array images
        LRGB = 5,

        /*
         * 20-26: Non-ASCOM bayer matrix types
         */

        BGGR = 20,
        GBRG = 21,
        GRBG = 22,
        GRGB = 23,
        GBGR = 24,
        RGBG = 25,
        BGRG = 26
    }
}