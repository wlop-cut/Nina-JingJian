#region "copyright"

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
using System.Runtime.Serialization;

namespace NINA.Profile {

    [Serializable()]
    [DataContract]
    public class TelescopeSettings : Settings, ITelescopeSettings {

        [OnDeserializing]
        public void OnDeserializing(StreamingContext context) {
            SetDefaultValues();
        }

        protected override void SetDefaultValues() {
            id = "No_Device";
            name = string.Empty;
            focalLength = double.NaN;
            focalRatio = double.NaN;
            snapPortStart = ":SNAP1,1#";
            snapPortStop = "SNAP1,0#";
            settleTime = 5;
            gid = "No_Device";
            gname = string.Empty;
            gfocalLength = double.NaN;
            gfocalRatio = double.NaN;
            gsnapPortStart = ":SNAP1,1#";
            gsnapPortStop = "SNAP1,0#";
            gsettleTime = 5;

            noSync = false;
        }

        private string id;

        [DataMember]
        public string Id {
            get {
                return id;
            }
            set {
                if (id != value) {
                    id = value;
                    RaisePropertyChanged();
                }
            }
        }

         private string gid;

        [DataMember]
        public string GId
        {
            get
            {
                return gid;
            }
            set
            {
                if (gid != value)
                {
                    gid = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string name;

        [DataMember]
        public string Name {
            get {
                return name;
            }
            set {
                if (name != value) {
                    name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string gname;

        [DataMember]
        public string GName
        {
            get
            {
                return gname;
            }
            set
            {
                if (gname != value)
                {
                    gname = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double focalLength;

        [DataMember]
        public double FocalLength {
            get {
                return focalLength;
            }
            set {
                if (focalLength != value) {
                    focalLength = value;
                    RaisePropertyChanged();
                }
            }
        }
       
       private double gfocalLength;

        [DataMember]
        public double GFocalLength
        {
            get
            {
                return gfocalLength;
            }
            set
            {
                if (gfocalLength != value)
                {
                    gfocalLength = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double focalRatio;

        [DataMember]
        public double FocalRatio {
            get {
                return focalRatio;
            }
            set {
                if (focalRatio != value) {
                    focalRatio = value;
                    RaisePropertyChanged();
                }
            }
        }

         private double gfocalRatio;

        [DataMember]
        public double GFocalRatio
        {
            get
            {
                return gfocalRatio;
            }
            set
            {
                if (gfocalRatio != value)
                {
                    gfocalRatio = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string snapPortStart;

        [DataMember]
        public string SnapPortStart {
            get {
                return snapPortStart;
            }
            set {
                if (snapPortStart != value) {
                    snapPortStart = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string gsnapPortStart;

        [DataMember]
        public string GSnapPortStart
        {
            get
            {
                return gsnapPortStart;
            }
            set
            {
                if (gsnapPortStart != value)
                {
                    gsnapPortStart = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string snapPortStop;

        [DataMember]
        public string SnapPortStop {
            get {
                return snapPortStop;
            }
            set {
                if (snapPortStop != value) {
                    snapPortStop = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string gsnapPortStop;

        [DataMember]
        public string GSnapPortStop
        {
            get
            {
                return gsnapPortStop;
            }
            set
            {
                if (gsnapPortStop != value)
                {
                    gsnapPortStop = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int settleTime;

        [DataMember]
        public int SettleTime {
            get {
                return settleTime;
            }
            set {
                if (settleTime != value) {
                    settleTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool noSync;

        [DataMember]
        public bool NoSync {
            get {
                return noSync;
            }
            set {
                if (noSync != value) {
                    noSync = value;
                    RaisePropertyChanged();
                }
            }
        }

         private int gsettleTime;

        [DataMember]
        public int GSettleTime
        {
            get
            {
                return gsettleTime;
            }
            set
            {
                if (gsettleTime != value)
                {
                    gsettleTime = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}