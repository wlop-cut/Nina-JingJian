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

namespace NINA.Profile {

    public interface ITelescopeSettings : ISettings {
        string Name { get; set; }
        double FocalLength { get; set; }
        double FocalRatio { get; set; }
        string Id { get; set; }
        int SettleTime { get; set; }
        string SnapPortStart { get; set; }
        string SnapPortStop { get; set; }
        bool NoSync { get; set; }

        string GName { get; set; }
        double GFocalLength { get; set; }
        double GFocalRatio { get; set; }
        string GId { get; set; }
        int GSettleTime { get; set; }
        string GSnapPortStart { get; set; }
        string GSnapPortStop { get; set; }

    }
}