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

using NINA.Model.ImageData;
using NINA.Profile;
using NINA.Utility;
using NINA.Utility.Notification;
using Omegon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Model.MyCamera {

    internal class OmegonCamera : BaseINPC, ICamera {
        private Omegonprocam.eFLAG flags;
        private Omegonprocam camera;

        public OmegonCamera(Omegonprocam.DeviceV2 instance, IProfileService profileService) {
            this.profileService = profileService;
            this.Id = instance.id;
            this.Name = instance.displayname;
            this.Description = instance.model.name;
            this.PixelSizeX = instance.model.xpixsz;
            this.PixelSizeY = instance.model.ypixsz;

            this.flags = (Omegonprocam.eFLAG)instance.model.flag;
        }

        private IProfileService profileService;

        public string Category { get; } = "Omegon";

        public bool HasShutter {
            get {
                return false;
            }
        }

        public double Temperature {
            get {
                camera.get_Temperature(out var temp);
                return temp / 10.0;
            }
        }

        public double TemperatureSetPoint {
            get {
                if (CanSetTemperature) {
                    camera.get_Option(Omegonprocam.eOPTION.OPTION_TECTARGET, out var target);
                    return target / 10.0;
                } else {
                    return double.NaN;
                }
            }
            set {
                if (CanSetTemperature) {
                    if (camera.put_Option(Omegonprocam.eOPTION.OPTION_TECTARGET, (int)(value * 10))) {
                        RaisePropertyChanged();
                    }
                }
            }
        }

        private short binX;

        public short BinX {
            get {
                return binX;
            }

            set {
                binX = value;
                RaisePropertyChanged();
            }
        }

        private short binY;

        public short BinY {
            get {
                return binY;
            }

            set {
                binY = value;
                RaisePropertyChanged();
            }
        }

        public string SensorName {
            get {
                return string.Empty;
            }
        }

        public SensorType SensorType { get; private set; }

        public short BayerOffsetX { get; private set; } = 0;

        public short BayerOffsetY { get; private set; } = 0;

        public int CameraXSize { get; private set; }

        public int CameraYSize { get; private set; }

        public double ExposureMin {
            get {
                camera.get_ExpTimeRange(out var min, out var max, out var def);
                return min / 1000000.0;
            }
        }

        public double ExposureMax {
            get {
                camera.get_ExpTimeRange(out var min, out var max, out var def);
                return max / 1000000.0;
            }
        }

        public double ElectronsPerADU => double.NaN;

        public short MaxBinX { get; private set; }

        public short MaxBinY { get; private set; }

        public double PixelSizeX { get; }

        public double PixelSizeY { get; }

        private bool canGetTemperature;

        public bool CanGetTemperature {
            get {
                return canGetTemperature;
            }
            private set {
                canGetTemperature = value;
                RaisePropertyChanged();
            }
        }

        private bool canSetTemperature;

        public bool CanSetTemperature {
            get {
                return canSetTemperature;
            }
            private set {
                canSetTemperature = value;
                RaisePropertyChanged();
            }
        }

        public bool CoolerOn {
            get {
                camera.get_Option(Omegonprocam.eOPTION.OPTION_TEC, out var cooler);
                return cooler == 1;
            }

            set {
                if (camera.put_Option(Omegonprocam.eOPTION.OPTION_TEC, value ? 1 : 0)) {
                    //Toggle fan, if supported
                    camera.put_Option(Omegonprocam.eOPTION.OPTION_FAN, value ? 1 : 0);
                    RaisePropertyChanged();
                }
            }
        }

        private double coolerPower = 0.0;

        public double CoolerPower {
            get {
                return coolerPower;
            }
            private set {
                coolerPower = value;
                RaisePropertyChanged();
            }
        }

        private CancellationTokenSource coolerPowerReadoutCts;

        /// <summary>
        /// This task will update cooler power based on TEC Volatage readout every three seconds
        /// Due to the fact that this value must not be updated more than every two seconds according to the documentation
        /// a helper method is required in case the device polling interval is faster than that.
        /// </summary>
        private void CoolerPowerUpdateTask() {
            Task.Run(async () => {
                coolerPowerReadoutCts?.Dispose();
                coolerPowerReadoutCts = new CancellationTokenSource();
                try {
                    camera.get_Option(Omegonprocam.eOPTION.OPTION_TEC_VOLTAGE_MAX, out var maxVoltage);
                    while (true) {
                        coolerPowerReadoutCts.Token.ThrowIfCancellationRequested();

                        camera.get_Option(Omegonprocam.eOPTION.OPTION_TEC_VOLTAGE, out var voltage);

                        CoolerPower = 100 * voltage / (double)maxVoltage;

                        //Recommendation to not readout CoolerPower in less than two seconds.
                        await Task.Delay(TimeSpan.FromSeconds(3), coolerPowerReadoutCts.Token);
                    }
                } catch (OperationCanceledException) {
                }
            });
        }

        public bool HasDewHeater {
            get {
                return false;
            }
        }

        public bool DewHeaterOn {
            get {
                return false;
            }
            set {
            }
        }

        public string CameraState {
            get {
                /* No State available */
                return string.Empty;
            }
        }

        public bool CanSubSample {
            get {
                /*
                 Currently Omegon Cameras are fixed to certain subsample resolutions, which is incompatible to NINA's approach
                 */
                return false;
            }
        }

        public bool EnableSubSample { get; set; }
        public int SubSampleX { get; set; }
        public int SubSampleY { get; set; }
        public int SubSampleWidth { get; set; }
        public int SubSampleHeight { get; set; }

        public bool CanShowLiveView {
            get {
                return true;
            }
        }

        private bool _liveViewEnabled;

        public bool LiveViewEnabled {
            get {
                return _liveViewEnabled;
            }
            set {
                _liveViewEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool HasBattery {
            get {
                return false;
            }
        }

        public int BatteryLevel {
            get {
                return -1;
            }
        }

        public int Offset {
            get {
                camera.get_Option(Omegonprocam.eOPTION.OPTION_BLACKLEVEL, out var level);
                return level;
            }
            set {
                if (!camera.put_Option(Omegonprocam.eOPTION.OPTION_BLACKLEVEL, value)) {
                } else {
                    RaisePropertyChanged();
                }
            }
        }

        public int OffsetMin {
            get {
                return 0;
            }
        }

        public int OffsetMax {
            get {
                return 31 * (1 << BitDepth - 8);
            }
        }

        public int USBLimit {
            get {
                camera.get_Speed(out var speed);
                return speed;
            }
            set {
                if (value >= USBLimitMin && value <= USBLimitMax) {
                    camera.put_Speed((ushort)value);
                    RaisePropertyChanged();
                }
            }
        }

        public int USBLimitMin {
            get {
                return 0;
            }
        }

        public int USBLimitMax {
            get {
                return (int)camera.MaxSpeed;
            }
        }

        private bool canSetOffset;

        public bool CanSetOffset {
            get {
                return canSetOffset;
            }
            set {
                canSetOffset = value;
                RaisePropertyChanged();
            }
        }

        public bool CanSetUSBLimit {
            get {
                return true;
            }
        }

        public bool CanGetGain {
            get {
                return camera.get_ExpoAGain(out var gain);
            }
        }

        public bool CanSetGain {
            get {
                return GainMax != GainMin;
            }
        }

        public int GainMax {
            get {
                camera.get_ExpoAGainRange(out var min, out var max, out var def);
                return max;
            }
        }

        public int GainMin {
            get {
                camera.get_ExpoAGainRange(out var min, out var max, out var def);
                return min;
            }
        }

        public int Gain {
            get {
                camera.get_ExpoAGain(out var gain);
                return gain;
            }

            set {
                if (value >= GainMin && value <= GainMax) {
                    camera.put_ExpoAGain((ushort)value);
                    RaisePropertyChanged();
                }
            }
        }

        public IEnumerable ReadoutModes => new List<string> { "Default" };

        public short ReadoutMode {
            get => 0;
            set { }
        }

        private short _readoutModeForSnapImages;

        public short ReadoutModeForSnapImages {
            get => _readoutModeForSnapImages;
            set {
                _readoutModeForSnapImages = value;
                RaisePropertyChanged();
            }
        }

        private short _readoutModeForNormalImages;

        public short ReadoutModeForNormalImages {
            get => _readoutModeForNormalImages;
            set {
                _readoutModeForNormalImages = value;
                RaisePropertyChanged();
            }
        }

        public ArrayList Gains {
            get {
                return new ArrayList();
            }
        }

        private AsyncObservableCollection<BinningMode> binningModes;

        public AsyncObservableCollection<BinningMode> BinningModes {
            get {
                if (binningModes == null) {
                    binningModes = new AsyncObservableCollection<BinningMode>();
                }
                return binningModes;
            }
            private set {
                binningModes = value;
                RaisePropertyChanged();
            }
        }

        public bool HasSetupDialog => false;

        private string id;

        public string Id {
            get {
                return id;
            }
            set {
                id = value;
                RaisePropertyChanged();
            }
        }

        private string name;

        public string Name {
            get {
                return name;
            }
            set {
                name = value;
                RaisePropertyChanged();
            }
        }

        private bool _connected;

        public bool Connected {
            get {
                return _connected;
            }
            set {
                _connected = value;
                if (!_connected) {
                    coolerPowerReadoutCts?.Cancel();
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasSetupDialog));
            }
        }

        private string description;

        public string Description {
            get {
                return description;
            }
            set {
                description = value;
                RaisePropertyChanged();
            }
        }

        public string DriverInfo {
            get {
                return "Omegon SDK";
            }
        }

        public string DriverVersion {
            get {
                return Omegonprocam.Version();
            }
        }

        public void AbortExposure() {
            StopExposure();
        }

        private void ReadOutBinning() {
            /* Found no way to readout available binning modes. Assume 4x4 for all cams for now */
            BinningModes.Clear();
            MaxBinX = 4;
            MaxBinY = 4;
            for (short i = 1; i <= MaxBinX; i++) {
                BinningModes.Add(new BinningMode(i, i));
            }
            BinX = 1;
            BinY = 1;
        }

        public Task<bool> Connect(CancellationToken ct) {
            return Task<bool>.Run(() => {
                var success = false;
                try {
                    imageReadyTCS?.TrySetCanceled();
                    imageReadyTCS = null;

                    camera = Omegonprocam.Open(this.Id);

                    success = true;

                    /* Use maximum bit depth */
                    if (!camera.put_Option(Omegonprocam.eOPTION.OPTION_BITDEPTH, 1)) {
                        throw new Exception("OmegonCamera - Could not set bit depth");
                    }

                    /* Use RAW Mode */
                    if (!camera.put_Option(Omegonprocam.eOPTION.OPTION_RAW, 1)) {
                        throw new Exception("OmegonCamera - Could not set RAW mode");
                    }

                    camera.put_AutoExpoEnable(false);

                    ReadOutBinning();

                    camera.get_Size(out var width, out var height);
                    this.CameraXSize = width;
                    this.CameraYSize = height;

                    /* Readout flags */
                    if ((this.flags & Omegonprocam.eFLAG.FLAG_TEC_ONOFF) != 0) {
                        /* Can set Target Temp */
                        CanSetTemperature = true;
                        TemperatureSetPoint = 20;
                        CoolerPowerUpdateTask();
                    }

                    if ((this.flags & Omegonprocam.eFLAG.FLAG_GETTEMPERATURE) != 0) {
                        /* Can get Target Temp */
                        CanGetTemperature = true;
                    }

                    if ((this.flags & Omegonprocam.eFLAG.FLAG_BLACKLEVEL) != 0) {
                        CanSetOffset = true;
                    }

                    if ((this.flags & Omegonprocam.eFLAG.FLAG_TRIGGER_SOFTWARE) == 0) {
                        throw new Exception("OmegonCamera - This camera is not capable to be triggered by software and is not supported");
                    }

                    if (!camera.put_Option(Omegonprocam.eOPTION.OPTION_TRIGGER, 1)) {
                        throw new Exception("OmegonCamera - Could not set Trigger manual mode");
                    }

                    if (!camera.StartPullModeWithCallback(new Omegonprocam.DelegateEventCallback(OnEventCallback))) {
                        throw new Exception("OmegonCamera - Could not start pull mode");
                    }

                    if (!camera.get_RawFormat(out var fourCC, out var bitDepth)) {
                        throw new Exception("OmegonCamera - Unable to get format information");
                    } else {
                        if (camera.MonoMode) {
                            SensorType = SensorType.Monochrome;
                        } else {
                            SensorType = GetSensorType(fourCC);
                        }
                    }

                    this.BitDepth = (int)bitDepth;

                    Connected = true;
                    RaiseAllPropertiesChanged();
                } catch (Exception ex) {
                    Logger.Error(ex);
                    Notification.ShowError(ex.Message);
                }
                return success;
            });
        }

        private SensorType GetSensorType(uint fourCC) {
            var bytes = BitConverter.GetBytes(fourCC);
            if (!BitConverter.IsLittleEndian) { Array.Reverse(bytes); }

            var sensor = System.Text.Encoding.ASCII.GetString(bytes);
            if (Enum.TryParse(sensor, true, out SensorType sensorType)) {
                return sensorType;
            }
            return SensorType.RGGB;
        }

        private void OnEventCallback(Omegonprocam.eEVENT nEvent) {
            Logger.Trace($"OmegonCamera - OnEventCallback {nEvent}");
            switch (nEvent) {
                case Omegonprocam.eEVENT.EVENT_IMAGE: // Live View Image
                    Logger.Trace($"OmegonCamera - Setting DownloadExposure Result on Task {imageReadyTCS.Task.Id}");
                    var success = imageReadyTCS?.TrySetResult(true);
                    Logger.Trace($"OmegonCamera - DownloadExposure Result on Task {imageReadyTCS.Task.Id} set successfully: {success}");
                    break;

                case Omegonprocam.eEVENT.EVENT_STILLIMAGE: // Still Image
                    Logger.Warning("OmegonCamera - Still image event received, but not expected to get one!");
                    imageReadyTCS?.TrySetResult(true);
                    break;

                case Omegonprocam.eEVENT.EVENT_TIMEOUT:
                    Logger.Error("OmegonCamera - Timout event occurred!");
                    break;

                case Omegonprocam.eEVENT.EVENT_TRIGGERFAIL:
                    Logger.Error("OmegonCamera - Trigger Fail event received!");
                    break;

                case Omegonprocam.eEVENT.EVENT_ERROR: // Error
                    Logger.Error("OmegonCamera - Camera reported a generic error!");
                    Notification.ShowError("Camera reported a generic error and needs to be reconnected!");
                    Disconnect();
                    break;

                case Omegonprocam.eEVENT.EVENT_DISCONNECTED:
                    Logger.Warning("OmegonCamera - Camera disconnected! Maybe USB connection was interrupted.");
                    Notification.ShowError("Camera disconnected! Maybe USB connection was interrupted.");
                    OnEventDisconnected();
                    break;
            }
        }

        private IExposureData PullImage() {
            /* peek the width and height */
            camera.get_Option(Omegonprocam.eOPTION.OPTION_BINNING, out var binning);
            var width = CameraXSize / binning;
            var height = CameraYSize / binning;

            var size = width * height;
            var data = new ushort[size];

            if (!camera.PullImageV2(data, BitDepth, out var info)) {
                Logger.Error("OmegonCamera - Failed to pull image");
                return null;
            }

            var bitScaling = this.profileService.ActiveProfile.CameraSettings.BitScaling;
            if (bitScaling) {
                var shift = 16 - BitDepth;
                for (var i = 0; i < data.Length; i++) {
                    data[i] = (ushort)(data[i] << shift);
                }
            }

            var imageData = new ImageArrayExposureData(
                    input: data,
                    width: width,
                    height: height,
                    bitDepth: bitScaling ? 16 : this.BitDepth,
                    isBayered: this.SensorType != SensorType.Monochrome,
                    metaData: new ImageMetaData());

            return imageData;
        }

        public void Disconnect() {
            coolerPowerReadoutCts?.Cancel();
            Connected = false;
            camera.Close();
            camera = null;
        }

        public async Task WaitUntilExposureIsReady(CancellationToken token) {
            using (token.Register(() => AbortExposure())) {
                await imageReadyTCS.Task;
            }
        }

        public async Task<IExposureData> DownloadExposure(CancellationToken token) {
            if (imageReadyTCS.Task.IsCanceled) { return null; }
            using (token.Register(() => imageReadyTCS.TrySetCanceled())) {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15))) {
                    using (cts.Token.Register(() => { Logger.Error("OmegonCamera - No Image Callback Event received"); imageReadyTCS.TrySetResult(true); })) {
                        var imageReady = await imageReadyTCS.Task;
                        return PullImage();
                    }
                }
            }
        }

        public Task<IExposureData> DownloadLiveView(CancellationToken token) {
            return DownloadExposure(token);
        }

        public void SetBinning(short x, short y) {
            if (x <= MaxBinX) {
                camera.put_Option(Omegonprocam.eOPTION.OPTION_BINNING, x);
            }
        }

        public void SetupDialog() {
        }

        /// <summary>
        /// Sets the exposure time. When given exposure time is out of bounds it will set it to nearest bound.
        /// </summary>
        /// <param name="time">Time in seconds</param>
        private void SetExposureTime(double time) {
            if (time < ExposureMin) {
                time = ExposureMin;
            }
            if (time > ExposureMax) {
                time = ExposureMax;
            }

            var µsTime = (uint)(time * 1000000);
            if (!camera.put_ExpoTime(µsTime)) {
                throw new Exception("OmegonCamera - Could not set exposure time");
            }
        }

        public void StartExposure(CaptureSequence sequence) {
            imageReadyTCS?.TrySetCanceled();
            imageReadyTCS = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Logger.Trace($"OmegonCamera - created new downloadExposure Task with Id {imageReadyTCS.Task.Id}");

            SetExposureTime(sequence.ExposureTime);

            if (!camera.Trigger(1)) {
                throw new Exception("OmegonCamera - Failed to trigger camera");
            }
        }

        private TaskCompletionSource<bool> imageReadyTCS;
        private int bitDepth;

        public int BitDepth {
            get {
                return bitDepth;
            }
            private set {
                bitDepth = value;
                RaisePropertyChanged();
            }
        }

        private void OnEventDisconnected() {
            StopExposure();
            Disconnect();
        }

        public void StartLiveView() {
            if (!camera.put_Option(Omegonprocam.eOPTION.OPTION_TRIGGER, 0)) {
                throw new Exception("OmegonCamera - Could not set Trigger video mode");
            }
            imageReadyTCS?.TrySetCanceled();
            imageReadyTCS = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            LiveViewEnabled = true;
        }

        public void StopExposure() {
            if (!camera.Trigger(0)) {
                Logger.Warning("OmegonCamera - Could not stop exposure");
            }
            imageReadyTCS.TrySetCanceled();
        }

        public void StopLiveView() {
            imageReadyTCS.Task.ContinueWith((Task<bool> o) => {
                if (!camera.put_Option(Omegonprocam.eOPTION.OPTION_TRIGGER, 1)) {
                    Disconnect();
                    throw new Exception("OmegonCamera - Could not set Trigger manual mode. Reconnect Camera!");
                }
                LiveViewEnabled = false;
            });
        }

        public int USBLimitStep { get => 1; }
    }
}