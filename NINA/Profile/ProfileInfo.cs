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

using NINA.Utility;
using System;

namespace NINA.Profile {

    public class ProfileMeta : BaseINPC {
        public Guid Id { get; set; }
        public DateTime LastUsed { get; set; }
        private string name;

        public string Name {
            get => name;
            set {
                name = value;
                RaisePropertyChanged();
            }
        }

        public string Location { get; set; }
        private bool isActive;

        public bool IsActive {
            get => isActive;
            set {
                isActive = value;
                RaisePropertyChanged();
            }
        }
    }
}