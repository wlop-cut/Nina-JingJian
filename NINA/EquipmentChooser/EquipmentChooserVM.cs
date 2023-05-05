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
using NINA.Model.MyFilterWheel;
using NINA.Utility;
using NINA.Profile;
using NINA.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using ZWOptical.ASISDK;

namespace NINA.EquipmentChooser {

    internal class EquipmentChooserVM : BaseVM {

        private EquipmentChooserVM(EquipmentType equipment, IProfileService profileService) : base(profileService) {
            if (equipment == EquipmentType.Camera) {
                GetCameras();
            } else if (equipment == EquipmentType.FilterWheel) {
                GetFilterWheels();
            }

            SetupDialogCommand = new RelayCommand(OpenSetupDialog);
        }

        private bool? _dialogResult;

        public bool? DialogResult {
            get {
                return _dialogResult;
            }
            set {
                _dialogResult = value;
                RaisePropertyChanged();
            }
        }

        private void OpenSetupDialog(object o) {
            if (SelectedDevice?.HasSetupDialog == true) {
                SelectedDevice.SetupDialog();
            }
        }

        private void GetFilterWheels() {
            using (var ascomDevices = new ASCOM.Utilities.Profile()) {
                foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("FilterWheel")) {
                    try {
                        AscomFilterWheel cam = new AscomFilterWheel(device.Key, device.Value);
                        Devices.Add(cam);
                    } catch (Exception) {
                        //only add filter wheels which are supported. e.g. x86 drivers will not work in x64
                    }
                }

                if (Devices.Count > 0) {
                    var selected = (from device in Devices where device.Id == profileService.ActiveProfile.FilterWheelSettings.Id select device).First();
                    SelectedDevice = selected;
                }
            }
        }

        //zhang096
        //private void GetTelescopes() {
        //    using (var ascomDevices = new ASCOM.Utilities.Profile()) {
        //        foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("Telescope")) {
        //            try {
        //                AscomTelescope cam = new AscomTelescope(device.Key, device.Value, profileService);
        //                Devices.Add(cam);
        //            } catch (Exception) {
        //                //only add telescopes which are supported. e.g. x86 drivers will not work in x64
        //            }
        //        }

        //        if (Devices.Count > 0) {
        //            var selected = (from device in Devices where device.Id == profileService.ActiveProfile.TelescopeSettings.Id select device).First();
        //            SelectedDevice = selected;
        //        }
        //    }
        //}

        [Obsolete]
        public static Model.IDevice Show(EquipmentType equipment, IProfileService profileService) {
            var chooser = new EquipmentChooserVM(equipment, profileService);

            System.Windows.Window win = new EquipmentChooser {
                Title = "Choose Equipment",
                DataContext = chooser
            };

            var mainwindow = System.Windows.Application.Current.MainWindow;
            mainwindow.Opacity = 0.8;
            win.Left = mainwindow.Left + (mainwindow.Width - win.Width) / 2; ;
            win.Top = mainwindow.Top + (mainwindow.Height - win.Height) / 2;

            win.ShowDialog();
            mainwindow.Opacity = 1;
            if (win.DialogResult.Value) {
                return chooser.SelectedDevice;
            } else {
                return null;
            }
        }

        private ObservableCollection<Model.IDevice> _devices;

        public ObservableCollection<Model.IDevice> Devices {
            get {
                if (_devices == null) {
                    _devices = new ObservableCollection<Model.IDevice>();
                }
                return _devices;
            }
            set {
                _devices = value;
            }
        }

        private Model.IDevice _selectedDevice;

        public Model.IDevice SelectedDevice {
            get {
                return _selectedDevice;
            }
            set {
                _selectedDevice = value;
                RaisePropertyChanged();
            }
        }

        private void GetCameras() {
            using (var ascomDevices = new ASCOM.Utilities.Profile()) {
                foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("Camera")) {
                    try {
                        AscomCamera cam = new AscomCamera(device.Key, "ASCOM --- " + device.Value, profileService);
                        Devices.Add(cam);
                    } catch (Exception) {
                        //only add cameras which are supported. e.g. x86 drivers will not work in x64
                    }
                }
            }
            for (int i = 0; i < ASICameras.Count; i++) {
                var cam = ASICameras.GetCamera(i, profileService);
                if (!string.IsNullOrEmpty(cam.Name)) {
                    Devices.Add(cam);
                }
            }
            /*IntPtr cameraList;
            uint err = EDSDK.EdsGetCameraList(out cameraList);
            if (err == EDSDK.EDS_ERR_OK) {
                int count;
                err = EDSDK.EdsGetChildCount(cameraList, out count);

                for(int i = 0; i < count; i++) {
                    IntPtr cam;
                    err = EDSDK.EdsGetChildAtIndex(cameraList, i, out cam);

                    EDSDK.EdsDeviceInfo info;
                    err = EDSDK.EdsGetDeviceInfo(cam, out info);

                    Devices.Add(new EDCamera(cam, info));*/
            /*err = EDSDK.EdsOpenSession(cam);

            IntPtr saveTo = (IntPtr)EDSDK.EdsSaveTo.Host;
            err = EDSDK.EdsSetPropertyData(cam, EDSDK.PropID_SaveTo, 0, 4, saveTo);

            EDSDK.EdsCapacity capacity = new EDSDK.EdsCapacity();
            capacity.NumberOfFreeClusters = 0x7FFFFFFF;
            capacity.BytesPerSector = 0x1000;
            capacity.Reset = 1;
            err = EDSDK.EdsSetCapacity(cam, capacity);

            err = EDSDK.EdsSendCommand(cam, EDSDK.CameraCommand_TakePicture, 0);

            err = EDSDK.EdsCloseSession(cam);*/
            /* }
         }*/

            if (Devices.Count > 0) {
                var items = (from device in Devices where device.Id == profileService.ActiveProfile.CameraSettings.Id select device);
                if (items.Count() > 0) {
                    SelectedDevice = items.First();
                } else {
                    SelectedDevice = Devices.First();
                }
            }
        }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum EquipmentType {

            [Description("LblCamera")]
            Camera,

            [Description("LblTelescope")]
            Telescope,

            [Description("LblFilterWheel")]
            FilterWheel
        }

        private ICommand _setupDialogCommand;

        public ICommand SetupDialogCommand {
            get {
                return _setupDialogCommand;
            }
            set {
                _setupDialogCommand = value;
            }
        }
    }
}