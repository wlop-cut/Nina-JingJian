#region "copyright"

/*
    Copyright ?2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com>

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

using Nikon;
using NINA.Utility;
using NINA.Utility.Enum;
using NINA.Utility.Mediator.Interfaces;
using NINA.Utility.Notification;
using NINA.Profile;
using NINA.Utility.RawConverter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using NINA.Model.ImageData;

namespace NINA.Model.MyCamera {

    internal class NikonCamera : BaseINPC, ICamera {

        public NikonCamera(IProfileService profileService) {
            this.profileService = profileService;
            /* NIKON */
            Name = "Nikon";
            _nikonManagers = new List<NikonManager>();
        }

        public string Category { get; } = "Nikon";

        private IProfileService profileService;
        private List<NikonManager> _nikonManagers;
        private NikonManager _activeNikonManager;

        private void Mgr_DeviceRemoved(NikonManager sender, NikonDevice device) {
            Disconnect();
        }

        private void Mgr_DeviceAdded(NikonManager sender, NikonDevice device) {
            var connected = false;
            try {
                _activeNikonManager = sender;
                _activeNikonManager.DeviceRemoved += Mgr_DeviceRemoved;

                Init(device);

                connected = true;
                Name = _camera.Name;
            } catch (Exception ex) {
                Notification.ShowError(ex.Message);
                Logger.Error(ex);
            } finally {
                Connected = connected;
                RaiseAllPropertiesChanged();
                _cameraConnected.TrySetResult(connected);
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

        public int BitDepth {
            get {
                return (int)profileService.ActiveProfile.CameraSettings.BitDepth;
            }
        }

        public void StartLiveView() {
            _camera.LiveViewEnabled = true;
            LiveViewEnabled = true;
        }

        public void StopLiveView() {
            _camera.LiveViewEnabled = false;
            LiveViewEnabled = false;
        }

        public Task<IExposureData> DownloadLiveView(CancellationToken token) {
            return Task.Run<IExposureData>(() => {
                byte[] buffer = _camera.GetLiveViewImage().JpegBuffer;
                using (var memStream = new MemoryStream(buffer)) {
                    memStream.Position = 0;

                    JpegBitmapDecoder decoder = new JpegBitmapDecoder(memStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad);

                    FormatConvertedBitmap bitmap = new FormatConvertedBitmap();
                    bitmap.BeginInit();
                    bitmap.Source = decoder.Frames[0];
                    bitmap.DestinationFormat = System.Windows.Media.PixelFormats.Gray16;
                    bitmap.EndInit();

                    ushort[] outArray = new ushort[bitmap.PixelWidth * bitmap.PixelHeight];
                    bitmap.CopyPixels(outArray, 2 * bitmap.PixelWidth, 0);

                    return new ImageArrayExposureData(
                            input: outArray,
                            width: bitmap.PixelWidth,
                            height: bitmap.PixelHeight,
                            bitDepth: 16,
                            isBayered: false,
                            metaData: new ImageMetaData());
                }
            });
        }

        private void CleanupUnusedManagers(NikonManager activeManager) {
            foreach (NikonManager mgr in _nikonManagers) {
                if (mgr != activeManager) {
                    mgr.Shutdown();
                }
            }
            _nikonManagers.Clear();
        }

        public void Init(NikonDevice cam) {
            Logger.Debug("Initializing Nikon camera");
            _camera = cam;
            _camera.ImageReady += Camera_ImageReady;
            _camera.CaptureComplete += _camera_CaptureComplete;

            //Set to shoot in RAW
            Logger.Debug("Setting compression to RAW");
            var compression = _camera.GetEnum(eNkMAIDCapability.kNkMAIDCapability_CompressionLevel);
            for (int i = 0; i < compression.Length; i++) {
                var val = compression.GetEnumValueByIndex(i);
                if (val.ToString() == "RAW") {
                    compression.Index = i;
                    _camera.SetEnum(eNkMAIDCapability.kNkMAIDCapability_CompressionLevel, compression);
                    break;
                }
            }

            GetShutterSpeeds();
            GetCapabilities();

            /* Setting SaveMedia when supported, to save images via SDRAM and not to the internal memory card */
            if (Capabilities.ContainsKey(eNkMAIDCapability.kNkMAIDCapability_SaveMedia) && Capabilities[eNkMAIDCapability.kNkMAIDCapability_SaveMedia].CanSet()) {
                _camera.SetUnsigned(eNkMAIDCapability.kNkMAIDCapability_SaveMedia, (uint)eNkMAIDSaveMedia.kNkMAIDSaveMedia_SDRAM);
            } else {
                Logger.Trace("Setting SaveMedia Capability not available. This has to be set manually or is not supported by this model.");
            }
        }

        private void GetCapabilities() {
            Logger.Debug("Getting Nikon capabilities");
            Capabilities.Clear();
            foreach (NkMAIDCapInfo info in _camera.GetCapabilityInfo()) {
                Capabilities.Add(info.ulID, info);

                var description = info.GetDescription();
                var canGet = info.CanGet();
                var canGetArray = info.CanGetArray();
                var canSet = info.CanSet();
                var canStart = info.CanStart();

                Logger.Debug(description);
                Logger.Debug("\t Id: " + info.ulID.ToString());
                Logger.Debug("\t CanGet: " + canGet.ToString());
                Logger.Debug("\t CanGetArray: " + canGetArray.ToString());
                Logger.Debug("\t CanSet: " + canSet.ToString());
                Logger.Debug("\t CanStart: " + canStart.ToString());

                if (info.ulID == eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed && !canSet) {
                    throw new NikonException("Cannot set shutterspeeds. Please make sure the camera dial is set to a position where bublb mode is possible and the mirror lock is turned off");
                }
            }
        }

        private Dictionary<eNkMAIDCapability, NkMAIDCapInfo> Capabilities = new Dictionary<eNkMAIDCapability, NkMAIDCapInfo>();

        private void GetShutterSpeeds() {
            Logger.Debug("Getting Nikon shutter speeds");
            _shutterSpeeds.Clear();
            var shutterSpeeds = _camera.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed);
            Logger.Debug("Available Shutterspeeds: " + shutterSpeeds.Length);
            bool bulbFound = false;
            for (int i = 0; i < shutterSpeeds.Length; i++) {
                try {
                    var val = shutterSpeeds.GetEnumValueByIndex(i).ToString();
                    Logger.Debug("Found Shutter speed: " + val);
                    if (val.Contains("/")) {
                        var split = val.Split('/');
                        var convertedSpeed = double.Parse(split[0], CultureInfo.InvariantCulture) / double.Parse(split[1], CultureInfo.InvariantCulture);

                        _shutterSpeeds.Add(i, convertedSpeed);
                    } else if (val.ToLower() == "bulb") {
                        Logger.Debug("Bulb index: " + i);
                        _bulbShutterSpeedIndex = i;
                        bulbFound = true;
                    } else if (val.ToLower() == "time") {
                        //currently unused
                    } else {
                        _shutterSpeeds.Add(i, double.Parse(val));
                    }
                } catch (Exception ex) {
                    Logger.Error("Unexpected Shutter Speed: ", ex);
                }
            }
            if (!bulbFound) {
                Logger.Error("No Bulb speed found!");
                throw new NikonException("Failed to find the 'Bulb' exposure mode");
            }
        }

        private TaskCompletionSource<object> _downloadExposure;
        private TaskCompletionSource<bool> _cameraConnected;

        private void _camera_CaptureComplete(NikonDevice sender, int data) {
            Logger.Debug("Capture complete");
        }

        private void Camera_ImageReady(NikonDevice sender, NikonImage image) {
            Logger.Debug("Image ready");
            _memoryStream = new MemoryStream(image.Buffer);
            Logger.Debug("Setting Download Exposure Task to complete");
            _downloadExposure.TrySetResult(null);
        }

        private NikonDevice _camera;

        public string Id {
            get {
                return "Nikon";
            }
        }

        private string _name;

        public string Name {
            get {
                return _name;
            }
            private set {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public bool CanShowLiveView {
            get {
                return _camera.SupportsCapability(eNkMAIDCapability.kNkMAIDCapability_GetLiveViewImage);
            }
        }

        public string Description {
            get {
                if (Connected) {
                    return _camera.Name;
                } else {
                    return string.Empty;
                }
            }
        }

        public bool HasShutter {
            get {
                return true;
            }
        }

        private bool _connected;

        public bool Connected {
            get {
                return _connected;
            }
            set {
                _connected = value;
                RaisePropertyChanged();
            }
        }

        public double Temperature {
            get {
                return double.NaN;
            }
        }

        public double TemperatureSetPoint {
            get {
                return double.NaN;
            }

            set {
            }
        }

        public short BinX {
            get {
                return 1;
            }

            set {
            }
        }

        public short BinY {
            get {
                return 1;
            }
            set {
            }
        }

        public string DriverInfo {
            get {
                return string.Empty;
            }
        }

        public string DriverVersion {
            get {
                return string.Empty;
            }
        }

        public string SensorName {
            get {
                return string.Empty;
            }
        }

        public SensorType SensorType {
            get {
                return SensorType.RGGB;
            }
        }

        public short BayerOffsetX => 0;

        public short BayerOffsetY => 0;

        public int CameraXSize {
            get {
                return -1;
            }
        }

        public int CameraYSize {
            get {
                return -1;
            }
        }

        public double ExposureMin {
            get {
                return 0;
            }
        }

        public double ExposureMax {
            get {
                return double.PositiveInfinity;
            }
        }

        public double ElectronsPerADU => double.NaN;

        public short MaxBinX {
            get {
                return 1;
            }
        }

        public short MaxBinY {
            get {
                return 1;
            }
        }

        public double PixelSizeX {
            get {
                return -1;
            }
        }

        public double PixelSizeY {
            get {
                return -1;
            }
        }

        public bool CanSetTemperature {
            get {
                return false;
            }
        }

        public bool CoolerOn {
            get {
                return false;
            }
            set {
            }
        }

        public double CoolerPower {
            get {
                return double.NaN;
            }
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

        private string _cameraState;

        public string CameraState {
            get {
                return _cameraState;
            }
            set {
                _cameraState = value;
                RaisePropertyChanged();
            }
        }

        public int Offset {
            get {
                return -1;
            }
            set {
            }
        }

        public int USBLimit {
            get {
                return -1;
            }
            set {
            }
        }

        public bool CanSetOffset {
            get {
                return false;
            }
        }

        public int OffsetMin {
            get {
                return 0;
            }
        }

        public int OffsetMax {
            get {
                return 0;
            }
        }

        public bool CanSetUSBLimit {
            get {
                return false;
            }
        }

        public bool CanSubSample {
            get {
                return false;
            }
        }

        public bool CanGetGain {
            get {
                if (Connected) {
                    return _camera.SupportsCapability(eNkMAIDCapability.kNkMAIDCapability_Sensitivity);
                } else {
                    return false;
                }
            }
        }

        public bool CanSetGain {
            get {
                if (Connected) {
                    return _camera.SupportsCapability(eNkMAIDCapability.kNkMAIDCapability_Sensitivity);
                } else {
                    return false;
                }
            }
        }

        public int GainMax {
            get {
                if (Gains != null) {
                    return ISOSpeeds.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                } else {
                    return 0;
                }
            }
        }

        public int GainMin {
            get {
                if (Gains != null) {
                    return ISOSpeeds.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
                } else {
                    return 0;
                }
            }
        }

        public int Gain {
            get {
                if (Connected) {
                    NikonEnum e = _camera.GetEnum(eNkMAIDCapability.kNkMAIDCapability_Sensitivity);
                    int iso;
                    if (int.TryParse(e.Value.ToString(), out iso)) {
                        return iso;
                    } else {
                        return -1;
                    }
                } else {
                    return -1;
                }
            }
            set {
                if (Connected) {
                    var iso = ISOSpeeds.Where((x) => x.Key == value).FirstOrDefault().Value;
                    NikonEnum e = _camera.GetEnum(eNkMAIDCapability.kNkMAIDCapability_Sensitivity);
                    e.Index = iso;
                    _camera.SetEnum(eNkMAIDCapability.kNkMAIDCapability_Sensitivity, e);
                    RaisePropertyChanged();
                }
            }
        }

        private Dictionary<int, int> ISOSpeeds = new Dictionary<int, int>();

        private ArrayList _gains;

        public ArrayList Gains {
            get {
                if (_gains == null) {
                    _gains = new ArrayList();
                }

                if (_gains.Count == 0 && Connected && CanGetGain) {
                    NikonEnum e = _camera.GetEnum(eNkMAIDCapability.kNkMAIDCapability_Sensitivity);
                    for (int i = 0; i < e.Length; i++) {
                        int iso;
                        if (int.TryParse(e.GetEnumValueByIndex(i).ToString(), out iso)) {
                            ISOSpeeds.Add(iso, i);
                            _gains.Add(iso);
                        }
                    }
                }
                return _gains;
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

        private AsyncObservableCollection<BinningMode> _binningModes;

        public AsyncObservableCollection<BinningMode> BinningModes {
            get {
                if (_binningModes == null) {
                    _binningModes = new AsyncObservableCollection<BinningMode>();
                    _binningModes.Add(new BinningMode(1, 1));
                }

                return _binningModes;
            }
        }

        public bool HasSetupDialog {
            get {
                return false;
            }
        }

        public bool EnableSubSample { get; set; }
        public int SubSampleX { get; set; }
        public int SubSampleY { get; set; }
        public int SubSampleWidth { get; set; }
        public int SubSampleHeight { get; set; }

        public int BatteryLevel {
            get {
                try {
                    return _camera.GetInteger(eNkMAIDCapability.kNkMAIDCapability_BatteryLevel);
                } catch (NikonException ex) {
                    Logger.Error(ex);
                    return -1;
                }
            }
        }

        public bool HasBattery => true;

        public void AbortExposure() {
            if (Connected) {
                _camera.StopBulbCapture();
            }
        }

        public void Disconnect() {
            Connected = false;
            _camera = null;
            _activeNikonManager?.Shutdown();
            _nikonManagers?.Clear();
            serialPortInteraction?.Close();
            serialPortInteraction = null;
        }

        public async Task WaitUntilExposureIsReady(CancellationToken token) {
            using (token.Register(() => AbortExposure())) {
                await _downloadExposure.Task;
            }
        }

        public async Task<IExposureData> DownloadExposure(CancellationToken token) {
            if (_downloadExposure.Task.IsCanceled) { return null; }
            Logger.Debug("Waiting for download of exposure");
            await _downloadExposure.Task;
            Logger.Debug("Downloading of exposure complete. Converting image to internal array");

            try {
                var rawImageData = _memoryStream.ToArray();
                var rawConverter = RawConverter.CreateInstance(profileService.ActiveProfile.CameraSettings.RawConverter);
                return new RAWExposureData(
                    rawConverter: rawConverter,
                    rawBytes: rawImageData,
                    rawType: "nef",
                    bitDepth: this.BitDepth,
                    metaData: new ImageMetaData());
            } finally {
                if (_memoryStream != null) {
                    _memoryStream.Dispose();
                    _memoryStream = null;
                }
            }
        }

        public void SetBinning(short x, short y) {
        }

        public void SetupDialog() {
        }

        private Dictionary<int, double> _shutterSpeeds = new Dictionary<int, double>();
        private int _bulbShutterSpeedIndex;

        public void StartExposure(CaptureSequence sequence) {
            if (Connected) {
                double exposureTime = sequence.ExposureTime;
                Logger.Debug("Prepare start of exposure: " + sequence);
                _downloadExposure = new TaskCompletionSource<object>();

                if (exposureTime <= 30.0) {
                    Logger.Debug("Exposuretime <= 30. Setting automatic shutter speed.");
                    var speed = _shutterSpeeds.Aggregate((x, y) => Math.Abs(x.Value - exposureTime) < Math.Abs(y.Value - exposureTime) ? x : y);
                    SetCameraShutterSpeed(speed.Key);

                    Logger.Debug("Start capture");
                    Task.Run(() => _camera.Capture());
                } else {
                    if (profileService.ActiveProfile.CameraSettings.BulbMode == CameraBulbModeEnum.SERIALPORT) {
                        Logger.Debug("Use Serial Port for camera");

                        BulbCapture(exposureTime, StartSerialPortCapture, StopSerialPortCapture);
                    } else if (profileService.ActiveProfile.CameraSettings.BulbMode == CameraBulbModeEnum.SERIALRELAY) {
                        Logger.Debug("Use serial relay for camera");

                        BulbCapture(exposureTime, StartSerialRelayCapture, StopSerialRelayCapture);
                    } else {
                        Logger.Debug("Use Bulb capture");
                        BulbCapture(exposureTime, StartBulbCapture, StopBulbCapture);
                    }
                }
            }
        }

        private SerialPortInteraction serialPortInteraction;
        private SerialRelayInteraction serialRelayInteraction;

        private void StartSerialRelayCapture() {
            Logger.Debug("Serial relay start of exposure");
            OpenSerialRelay();
            serialRelayInteraction.Send(new byte[] { 0xFF, 0x01, 0x01 });
        }

        private void StopSerialRelayCapture() {
            Logger.Debug("Serial relay stop of exposure");
            OpenSerialRelay();
            serialRelayInteraction.Send(new byte[] { 0xFF, 0x01, 0x00 });
        }

        private void StartSerialPortCapture() {
            Logger.Debug("Serial port start of exposure");
            OpenSerialPort();
            serialPortInteraction.EnableRts(true);
        }

        private void StopSerialPortCapture() {
            Logger.Debug("Serial port stop of exposure");
            OpenSerialPort();
            serialPortInteraction.EnableRts(false);
        }

        private void OpenSerialPort() {
            if (serialPortInteraction?.PortName != profileService.ActiveProfile.CameraSettings.SerialPort) {
                serialPortInteraction = new SerialPortInteraction(profileService.ActiveProfile.CameraSettings.SerialPort);
            }
            if (!serialPortInteraction.Open()) {
                throw new Exception("Unable to open SerialPort " + profileService.ActiveProfile.CameraSettings.SerialPort);
            }
        }

        private void OpenSerialRelay() {
            if (serialRelayInteraction?.PortName != profileService.ActiveProfile.CameraSettings.SerialPort) {
                serialRelayInteraction = new SerialRelayInteraction(profileService.ActiveProfile.CameraSettings.SerialPort);
            }
            if (!serialRelayInteraction.Open()) {
                throw new Exception("Unable to open SerialPort " + profileService.ActiveProfile.CameraSettings.SerialPort);
            }
        }

        //zhang048
        //private void RequestSnapPortCaptureStart() {
        //    Logger.Debug("Request start of exposure");
        //    var success = telescopeMediator.SendToSnapPort(true);
        //    if (!success) {
        //        throw new Exception("Request to telescope snap port failed");
        //    }
        //}
        //private void RequestSnapPortCaptureStop() {
        //    Logger.Debug("Request stop of exposure");
        //    var success = telescopeMediator.SendToSnapPort(false);
        //    if (!success) {
        //        throw new Exception("Request to telescope snap port failed");
        //    }
        //}

        private void BulbCapture(double exposureTime, Action capture, Action stopCapture) {
            SetCameraToManual();

            SetCameraShutterSpeed(_bulbShutterSpeedIndex);

            try {
                Logger.Debug("Starting bulb capture");
                capture();
            } catch (NikonException ex) {
                if (ex.ErrorCode != eNkMAIDResult.kNkMAIDResult_BulbReleaseBusy) {
                    throw;
                }
            }

            /*Stop Exposure after exposure time */
            Task.Run(async () => {
                await Utility.Utility.Wait(TimeSpan.FromSeconds(exposureTime));

                stopCapture();

                Logger.Debug("Restore previous shutter speed");
                // Restore original shutter speed
                SetCameraShutterSpeed(_prevShutterSpeed);
            });
        }

        private void StartBulbCapture() {
            _camera.StartBulbCapture();
        }

        private void StopBulbCapture() {
            _camera.StopBulbCapture();
        }

        private void LockCamera(bool lockIt) {
            Logger.Debug("Lock camera: " + lockIt);
            var lockCameraCap = eNkMAIDCapability.kNkMAIDCapability_LockCamera;
            _camera.SetBoolean(lockCameraCap, lockIt);
        }

        private void SetCameraToManual() {
            Logger.Debug("Set camera to manual exposure");
            if (Capabilities.ContainsKey(eNkMAIDCapability.kNkMAIDCapability_ExposureMode) && Capabilities[eNkMAIDCapability.kNkMAIDCapability_ExposureMode].CanSet()) {
                var exposureMode = _camera.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ExposureMode);
                var foundManual = false;
                for (int i = 0; i < exposureMode.Length; i++) {
                    if ((uint)exposureMode[i] == (uint)eNkMAIDExposureMode.kNkMAIDExposureMode_Manual) {
                        exposureMode.Index = i;
                        foundManual = true;
                        _camera.SetEnum(eNkMAIDCapability.kNkMAIDCapability_ExposureMode, exposureMode);
                        break;
                    }
                }

                if (!foundManual) {
                    throw new NikonException("Failed to find the 'Manual' exposure mode");
                }
            } else {
                Logger.Debug("Cannot set to manual mode. Skipping...");
            }
        }

        private int _prevShutterSpeed;
        private MemoryStream _memoryStream;

        private void SetCameraShutterSpeed(int index) {
            if (Capabilities.ContainsKey(eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed) && Capabilities[eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed].CanSet()) {
                Logger.Debug("Setting shutter speed to index: " + index);
                var shutterspeed = _camera.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed);
                _prevShutterSpeed = shutterspeed.Index;
                shutterspeed.Index = index;
                _camera.SetEnum(eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed, shutterspeed);
            } else {
                Logger.Debug("Cannot set camera shutter speed. Skipping...");
            }
        }

        public void StopExposure() {
            if (Connected) {
                _camera.StopBulbCapture();
            }
        }

        public async Task<bool> Connect(CancellationToken token) {
            return await Task.Run(() => {
                var connected = false;
                try {
                    serialPortInteraction = null;
                    _nikonManagers.Clear();

                    string architecture = "x64";
                    if (DllLoader.IsX86()) {
                        architecture = "x86";
                    }

                    var md3Folder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "External", architecture, "Nikon");

                    foreach (string file in Directory.GetFiles(md3Folder, "*.md3", SearchOption.AllDirectories)) {
                        NikonManager mgr = new NikonManager(file);
                        mgr.DeviceAdded += Mgr_DeviceAdded;
                        _nikonManagers.Add(mgr);
                    }

                    _cameraConnected = new TaskCompletionSource<bool>();
                    var d = DateTime.Now;

                    do {
                        token.ThrowIfCancellationRequested();
                        Thread.Sleep(500);
                    } while (!_cameraConnected.Task.IsCompleted);

                    connected = _cameraConnected.Task.Result;
                } catch (OperationCanceledException) {
                    _activeNikonManager = null;
                } finally {
                    CleanupUnusedManagers(_activeNikonManager);
                }

                return connected;
            });
        }
    }
}