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

namespace NINA.Utility.Mediator {

    public class PrepareImageParameters {
        public bool? AutoStretch { get; private set; }
        public bool? DetectStars { get; private set; }

        public PrepareImageParameters(bool? autoStretch = null, bool? detectStars = null) {
            this.AutoStretch = autoStretch;//自动拉伸，即增强对比度，把0-1变为0-255。有很多stretch的方法可以选择，如伽马变换
            this.DetectStars = detectStars;//通过对星星亮度进行测量，实时识别星星。可以学习一下其算法实现，针对某一个特征的识别。
        }
    }
}