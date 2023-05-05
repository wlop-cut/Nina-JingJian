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
using System.Xml;
using System.Xml.Serialization;

namespace NINA.Model {

    [Serializable()]
    [XmlRoot(ElementName = "CaptureSequence")]
    public class CaptureSequence : BaseINPC {

        public static class ImageTypes {
            public const string LIGHT = "LIGHT";
            public const string FLAT = "FLAT";
            public const string DARK = "DARK";
            public const string BIAS = "BIAS";
            public const string DARKFLAT = "DARKFLAT";
            public const string SNAPSHOT = "SNAPSHOT";
        }

        public CaptureSequence() {
            ExposureTime = 1;
            ImageType = ImageTypes.LIGHT;
            TotalExposureCount = 1;
            Dither = false;
            DitherAmount = 1;
            Gain = -1;
            Offset = -1;
        }

        public override string ToString() {
            return TotalExposureCount.ToString() + "x" + ExposureTime.ToString() + " " + ImageType;
        }

        public CaptureSequence(double exposureTime, string imageType, MyFilterWheel.FilterInfo filterType, MyCamera.BinningMode binning, int exposureCount) {
            ExposureTime = exposureTime;
            ImageType = imageType;
            FilterType = filterType;
            Binning = binning;//四个像素合成一个像素，加快处理像素速度
            TotalExposureCount = exposureCount;//成像次数即照相次数
            DitherAmount = 1;//dither是一个导星算法相关的参数，不需要管
            Gain = -1;//放大倍数。只是对电荷信号进行放大，并不影响清晰度（增大曝光时间，才能增加底层电荷量，从根本上提升清晰度）
            Offset = -1;//偏置。Gain和Offset都是后面电路的参数。
            Enabled = true;
        }

        public CaptureSequence Clone() {
            CaptureSequence clone = new CaptureSequence(ExposureTime, ImageType, FilterType, Binning, TotalExposureCount);
            clone.Gain = Gain;
            clone.Dither = Dither;
            clone.DitherAmount = DitherAmount;
            clone.Offset = Offset;
            return clone;
        }

        public bool IsLightSequence() {
            return ImageType == CaptureSequence.ImageTypes.SNAPSHOT || ImageType == CaptureSequence.ImageTypes.LIGHT;
        }

        public bool IsFlatSequence() {
            return ImageType == CaptureSequence.ImageTypes.FLAT;
        }

        public bool IsDarkSequence() {
            return ImageType == CaptureSequence.ImageTypes.DARKFLAT || ImageType == CaptureSequence.ImageTypes.BIAS || ImageType == CaptureSequence.ImageTypes.DARK;
        }

        private double _exposureTime;
        private string _imageType;
        private MyFilterWheel.FilterInfo _filterType;
        private MyCamera.BinningMode _binning;
        private int _progressExposureCount;

        [XmlElement(nameof(Enabled))]
        public bool Enabled {
            get {
                return _enabled;
            }
            set {
                _enabled = value;
                RaisePropertyChanged();
            }
        }

        [XmlElement(nameof(ExposureTime))]
        public double ExposureTime {
            get {
                return _exposureTime;
            }

            set {
                _exposureTime = value;
                RaisePropertyChanged();
            }
        }

        [XmlElement(nameof(ImageType))]
        public string ImageType {
            get {
                return _imageType;
            }

            set {
                _imageType = value;
                RaisePropertyChanged();
            }
        }

        [XmlElement(nameof(FilterType))]
        public Model.MyFilterWheel.FilterInfo FilterType {
            get {
                return _filterType;
            }

            set {
                _filterType = value;
                RaisePropertyChanged();
            }
        }

        [XmlElement(nameof(Binning))]
        public MyCamera.BinningMode Binning {
            get {
                if (_binning == null) {
                    _binning = new MyCamera.BinningMode(1, 1);
                }
                return _binning;
            }

            set {
                _binning = value;
                RaisePropertyChanged();
            }
        }

        private int _gain;

        [XmlElement(nameof(Gain))]
        public int Gain {
            get {
                return _gain;
            }
            set {
                _gain = value;
                RaisePropertyChanged();
            }
        }

        private int _offset;

        [XmlElement(nameof(Offset))]
        public int Offset {
            get {
                return _offset;
            }
            set {
                _offset = value;
                RaisePropertyChanged();
            }
        }

        private bool _enableSubSample = false;

        [XmlIgnore]
        public bool EnableSubSample {
            get {
                return _enableSubSample;
            }
            set {
                _enableSubSample = value;
                RaisePropertyChanged();
            }
        }

        private int _totalExposureCount;

        /// <summary>
        /// Total exposures of a sequence
        /// </summary>
        [XmlElement(nameof(TotalExposureCount))]
        public int TotalExposureCount {
            get {
                return _totalExposureCount;
            }
            set {
                _totalExposureCount = value;
                if (_totalExposureCount < ProgressExposureCount && _totalExposureCount >= 0) {
                    ProgressExposureCount = _totalExposureCount;
                }
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Number of exposures already taken
        /// </summary>
        [XmlElement(nameof(ProgressExposureCount))]
        public int ProgressExposureCount {
            get {
                return _progressExposureCount;
            }
            set {
                _progressExposureCount = value;
                if (ProgressExposureCount > TotalExposureCount) {
                    TotalExposureCount = ProgressExposureCount;
                }
                RaisePropertyChanged();
            }
        }

        private bool _dither;

        [XmlElement(nameof(Dither))]
        public bool Dither {
            get {
                return _dither;
            }
            set {
                _dither = value;
                RaisePropertyChanged();
            }
        }

        private int _ditherAmount;
        private bool _enabled = true;

        [XmlElement(nameof(DitherAmount))]
        public int DitherAmount {
            get {
                return _ditherAmount;
            }
            set {
                _ditherAmount = value;
                RaisePropertyChanged();
            }
        }

        private CaptureSequence nextSequence;

        [XmlIgnore]
        public CaptureSequence NextSequence {
            get => nextSequence;
            set {
                nextSequence = value;
                RaisePropertyChanged();
            }
        }
    }
}