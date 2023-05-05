#region "copyright"

/*
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

/*
 * Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com>
 * Copyright 2019 Dale Ghent <daleg@elemental.org>
 */

#endregion "copyright"

using NINA.Profile;
using NINA.Utility;
using NINA.Utility.Mediator;
using NINA.Utility.Mediator.Interfaces;
using NINA.Utility.Notification;
using NINA.ViewModel.Equipment.Camera;
using NINA.ViewModel.Equipment.FilterWheel;
using NINA.ViewModel.Imaging;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NINA.ViewModel {

    internal class ApplicationVM : BaseVM {

        public ApplicationVM() : this(new ProfileService()) {
        }

        public ApplicationVM(IProfileService profileService) : base(profileService) {
            if (NINA.Properties.Settings.Default.UpdateSettings) {
                NINA.Properties.Settings.Default.Upgrade();
                NINA.Properties.Settings.Default.UpdateSettings = false;
                NINA.Properties.Settings.Default.Save();
            }

            Logger.SetLogLevel(profileService.ActiveProfile.ApplicationSettings.LogLevel);
            cameraMediator = new CameraMediator();
            //zhang046
            //telescopeMediator = new TelescopeMediator();
            //zhang020
            //focuserMediator = new FocuserMediator();
            filterWheelMediator = new FilterWheelMediator();
            //zhang036
            //rotatorMediator = new RotatorMediator();
            //zhang059
            //guiderMediator = new GuiderMediator();
            imagingMediator = new ImagingMediator();
            applicationStatusMediator = new ApplicationStatusMediator();
            //zhang015
            //switchMediator = new SwitchMediator();
            //zhang041
            //weatherDataMediator = new WeatherDataMediator();

            //zhang015
            //SwitchVM = new SwitchVM(profileService, applicationStatusMediator, switchMediator);

            ExitCommand = new RelayCommand(ExitApplication);
            ClosingCommand = new RelayCommand(ClosingApplication);
            MinimizeWindowCommand = new RelayCommand(MinimizeWindow);
            MaximizeWindowCommand = new RelayCommand(MaximizeWindow);
            CheckProfileCommand = new RelayCommand(LoadProfile);
            CheckUpdateCommand = new AsyncCommand<bool>(() => CheckUpdate());
            OpenManualCommand = new RelayCommand(OpenManual);
            ConnectAllDevicesCommand = new AsyncCommand<bool>(async () => {
                var diag = MyMessageBox.MyMessageBox.Show(Locale.Loc.Instance["LblReconnectAll"], "", MessageBoxButton.OKCancel, MessageBoxResult.Cancel);
                if (diag == MessageBoxResult.OK) {
                    return await Task<bool>.Run(async () => {
                        var cam = cameraMediator.Connect();
                        var fw = filterWheelMediator.Connect();
                        //zhang049
                        //var telescope = telescopeMediator.Connect();
                        //zhang024
                        //var focuser = focuserMediator.Connect();
                        //zhang038
                        //var rotator = rotatorMediator.Connect();
                        //zhang071
                        //var guider = guiderMediator.Connect();
                        //zhang043
                        //var weather = weatherDataMediator.Connect();
                        //zhang015
                        //var swtch = switchMediator.Connect();
                        await Task.WhenAll(cam, fw);
                        return true;
                    });
                } else {
                    return false;
                }
            });
            DisconnectAllDevicesCommand = new RelayCommand((object o) => {
                var diag = MyMessageBox.MyMessageBox.Show(Locale.Loc.Instance["LblDisconnectAll"], "", MessageBoxButton.OKCancel, MessageBoxResult.Cancel);
                if (diag == MessageBoxResult.OK) {
                    DisconnectEquipment();
                }
            });

            InitAvalonDockLayout();

            OptionsVM.PropertyChanged += OptionsVM_PropertyChanged;

            profileService.ProfileChanged += ProfileService_ProfileChanged;
        }

        public IProfile ActiveProfile {
            get {
                return profileService.ActiveProfile;
            }
        }

        private void ProfileService_ProfileChanged(object sender, EventArgs e) {
            RaisePropertyChanged(nameof(ActiveProfile));
        }

        private async void OptionsVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(OptionsVM.AutoUpdateSource)) {
                await CheckUpdate();
            }
        }

        private ICameraMediator cameraMediator;
        //zhang049
        //private ITelescopeMediator telescopeMediator;
        //zhang024
        //private IFocuserMediator focuserMediator;
        private IFilterWheelMediator filterWheelMediator;
        //zhang036
        //private RotatorMediator rotatorMediator;
        //zhang068
        //private IGuiderMediator guiderMediator;
        private IImagingMediator imagingMediator;
        private IApplicationStatusMediator applicationStatusMediator;
        //zhang015
        //private SwitchMediator switchMediator;
        //zhang042
        //private IWeatherDataMediator weatherDataMediator;

        private void LoadProfile(object obj) {
            if (profileService.Profiles.Count > 1) {
                new ProfileSelectVM(profileService).SelectProfile();
            }
        }

        private Task<bool> CheckUpdate() {
            return VersionCheckVM.CheckUpdate();
        }

        private void OpenManual(object o) {
            System.Diagnostics.Process.Start("https://nighttime-imaging.eu/docs/master/site/");
        }

        public void InitAvalonDockLayout() {
            DockManagerVM.Anchorables.Add(ImagingVM.ImageControl);
            DockManagerVM.Anchorables.Add(CameraVM);
            DockManagerVM.Anchorables.Add(FilterWheelVM);
            //zhang020
            //DockManagerVM.Anchorables.Add(FocuserVM);
            //zhang029
            //DockManagerVM.Anchorables.Add(RotatorVM);
            //zhang045
            //DockManagerVM.Anchorables.Add(TelescopeVM);
            //zhang058
            //DockManagerVM.Anchorables.Add(GuiderVM);
            //zhang016
            //DockManagerVM.Anchorables.Add(SwitchVM);
            //zhang041
            //DockManagerVM.Anchorables.Add(WeatherDataVM);

            DockManagerVM.Anchorables.Add(SeqVM);
            DockManagerVM.Anchorables.Add(ImagingVM.ImgStatisticsVM);
            DockManagerVM.Anchorables.Add(SeqVM.ImgHistoryVM);

            DockManagerVM.Anchorables.Add(AnchorableSnapshotVM);
            DockManagerVM.Anchorables.Add(ThumbnailVM);
            //zhang041
            //DockManagerVM.Anchorables.Add(WeatherDataVM);
            //zhang080
            //DockManagerVM.Anchorables.Add(PlatesolveVM);
            //zhang085
            //DockManagerVM.Anchorables.Add(PolarAlignVM);
            //zhang101
            //DockManagerVM.Anchorables.Add(AutoFocusVM);
            //zhang099
            //DockManagerVM.Anchorables.Add(FocusTargetsVM);
            //zhang089
            //DockManagerVM.Anchorables.Add(ExposureCalculatorVM);

            DockManagerVM.AnchorableInfoPanels.Add(ImagingVM.ImageControl);
            DockManagerVM.AnchorableInfoPanels.Add(CameraVM);
            DockManagerVM.AnchorableInfoPanels.Add(FilterWheelVM);
            //zhang020
            //DockManagerVM.AnchorableInfoPanels.Add(FocuserVM);
            //zhang029
            //DockManagerVM.AnchorableInfoPanels.Add(RotatorVM);
            //zhang045
            //DockManagerVM.AnchorableInfoPanels.Add(TelescopeVM);
            //zhang058
            //DockManagerVM.AnchorableInfoPanels.Add(GuiderVM);
            DockManagerVM.AnchorableInfoPanels.Add(SeqVM);
            //zhang016
            //DockManagerVM.AnchorableInfoPanels.Add(SwitchVM);
            //zhang041
            //DockManagerVM.AnchorableInfoPanels.Add(WeatherDataVM);
            DockManagerVM.AnchorableInfoPanels.Add(ImagingVM.ImgStatisticsVM);
            DockManagerVM.AnchorableInfoPanels.Add(SeqVM.ImgHistoryVM);

            DockManagerVM.AnchorableTools.Add(AnchorableSnapshotVM);
            DockManagerVM.AnchorableTools.Add(ThumbnailVM);
            //zhang080
            //DockManagerVM.AnchorableTools.Add(PlatesolveVM);
            //zhang085
            //DockManagerVM.AnchorableTools.Add(PolarAlignVM);
            //zhang101
            //DockManagerVM.AnchorableTools.Add(AutoFocusVM);
            //zhang099
            //DockManagerVM.AnchorableTools.Add(FocusTargetsVM);
            //zhang089
            //DockManagerVM.AnchorableTools.Add(ExposureCalculatorVM);
        }

        public void ChangeTab(ApplicationTab tab) {
            TabIndex = (int)tab;
        }

        public string Version {
            get {
                return new ProjectVersion(Utility.Utility.Version).ToString();
            }
        }

        public string Title {
            get {
                return Utility.Utility.Title;
            }
        }

        private int _tabIndex;

        public int TabIndex {
            get {
                return _tabIndex;
            }
            set {
                _tabIndex = value;
                RaisePropertyChanged();
            }
        }

        public VersionCheckVM VersionCheckVM { get; private set; } = new VersionCheckVM();

        private ApplicationStatusVM _applicationStatusVM;

        public ApplicationStatusVM ApplicationStatusVM {
            get {
                if (_applicationStatusVM == null) {
                    _applicationStatusVM = new ApplicationStatusVM(profileService, applicationStatusMediator);
                }
                return _applicationStatusVM;
            }
            set {
                _applicationStatusVM = value;
                RaisePropertyChanged();
            }
        }

        private static void MaximizeWindow(object obj) {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized) {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            } else {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        private void MinimizeWindow(object obj) {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ExitApplication(object obj) {
            if (SeqVM?.OKtoExit() == false)
                return;
            if (CameraVM?.Cam?.Connected == true) {
                var diag = MyMessageBox.MyMessageBox.Show("Camera still connected. Exit anyway?", "", MessageBoxButton.OKCancel, MessageBoxResult.Cancel);
                if (diag != MessageBoxResult.OK) {
                    return;
                }
            }
           
            Application.Current.Shutdown();
          

        }

        private void ClosingApplication(object o) {
            try {
                DockManagerVM.SaveAvalonDockLayout();
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            DisconnectEquipment();

            Notification.Dispose();

            try {
                Utility.AtikSDK.AtikCameraDll.Shutdown();
            } catch (Exception) { }
        }

        public void DisconnectEquipment() {
            try {
                cameraMediator.Disconnect();
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            //zhang049
            //try {
            //    telescopeMediator.Disconnect();
            //} catch (Exception ex) {
            //    Logger.Error(ex);
            //}

            try {
                filterWheelMediator.Disconnect();
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            //zhang024
            //try {
            //    focuserMediator.Disconnect();
            //} catch (Exception ex) {
            //    Logger.Error(ex);
            //}

            //zhang038
            //try {
            //    rotatorMediator.Disconnect();
            //} catch (Exception ex) {
            //    Logger.Error(ex);
            //}

            //zhang068
            //try {
            //    guiderMediator.Disconnect();
            //} catch (Exception ex) {
            //    Logger.Error(ex);
            //}

            //zhang043
            //try {
            //    weatherDataMediator.Disconnect();
            //} catch (Exception ex) {
            //    Logger.Error(ex);
            //}
        }

        private DockManagerVM _dockManagerVM;

        public DockManagerVM DockManagerVM {
            get {
                if (_dockManagerVM == null) {
                    _dockManagerVM = new DockManagerVM(profileService);
                }
                return _dockManagerVM;
            }
            set {
                _dockManagerVM = value;
                RaisePropertyChanged();
            }
        }

        private ThumbnailVM _thumbnailVM;

        public ThumbnailVM ThumbnailVM {
            get {
                if (_thumbnailVM == null) {
                    _thumbnailVM = new ThumbnailVM(profileService, imagingMediator);
                }
                return _thumbnailVM;
            }
            set {
                _thumbnailVM = value;
                RaisePropertyChanged();
            }
        }

        private CameraVM _cameraVM;

        public CameraVM CameraVM {
            get {
                if (_cameraVM == null) {
                    _cameraVM = new CameraVM(profileService, cameraMediator, applicationStatusMediator);
                }
                return _cameraVM;
            }
            set {
                _cameraVM = value;
                RaisePropertyChanged();
            }
        }

        //zhang014
        //public SwitchVM SwitchVM { get; private set; }

        private FilterWheelVM _filterWheelVM;

        public FilterWheelVM FilterWheelVM {
            get {
                if (_filterWheelVM == null) {
                    _filterWheelVM = new FilterWheelVM(profileService, filterWheelMediator,  applicationStatusMediator);
                }
                return _filterWheelVM;
            }
            set {
                _filterWheelVM = value;
                RaisePropertyChanged();
            }
        }

        //zhang020
        //private FocuserVM _focuserVM;

        //public FocuserVM FocuserVM {
        //    get {
        //        if (_focuserVM == null) {
        //            _focuserVM = new FocuserVM(profileService, focuserMediator, applicationStatusMediator);
        //        }
        //        return _focuserVM;
        //    }
        //    set {
        //        _focuserVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //zhang029
        //private RotatorVM rotatorVM;

        //public RotatorVM RotatorVM {
        //    get {
        //        if (rotatorVM == null) {
        //            rotatorVM = new RotatorVM(profileService, rotatorMediator, applicationStatusMediator);
        //        }
        //        return rotatorVM;
        //    }
        //    set {
        //        rotatorVM = value;
        //        RaisePropertyChanged();
        //    }
        //}


        //zhang041
        //private WeatherDataVM _weatherDataVM;

        //public WeatherDataVM WeatherDataVM {
        //    get {
        //        if (_weatherDataVM == null) {
        //            _weatherDataVM = new WeatherDataVM(profileService, weatherDataMediator, applicationStatusMediator);
        //        }
        //        return _weatherDataVM;
        //    }
        //    set {
        //        _weatherDataVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //zhang006
        //private FlatWizardVM _flatWizardVM;

        //public FlatWizardVM FlatWizardVM {
        //    get =>
        //        _flatWizardVM ?? (_flatWizardVM = new FlatWizardVM(profileService,
        //            new ImagingVM(profileService, new ImagingMediator(), cameraMediator, telescopeMediator,
        //                filterWheelMediator, focuserMediator, rotatorMediator, guiderMediator,
        //                weatherDataMediator, applicationStatusMediator),
        //            cameraMediator,
        //            filterWheelMediator,
        //            telescopeMediator,
        //            flatDeviceMediator,
        //            new ApplicationResourceDictionary(),
        //            applicationStatusMediator));
        //    set {
        //        _flatWizardVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        private SequenceVM _seqVM;

        public SequenceVM SeqVM {
            get => _seqVM ?? (_seqVM = new SequenceVM(profileService, cameraMediator,filterWheelMediator, imagingMediator, applicationStatusMediator));
            set {
                _seqVM = value;
                RaisePropertyChanged();
            }
        }

        private ImagingVM _imagingVM;

        public ImagingVM ImagingVM {
            get {
                if (_imagingVM == null) {
                    _imagingVM = new ImagingVM(profileService, imagingMediator, cameraMediator, filterWheelMediator, applicationStatusMediator);
                }
                return _imagingVM;
            }
            set {
                _imagingVM = value;
                RaisePropertyChanged();
            }
        }

        private AnchorableSnapshotVM _anchorableSnapshotVM;

        public AnchorableSnapshotVM AnchorableSnapshotVM {
            get {
                if (_anchorableSnapshotVM == null) {
                    _anchorableSnapshotVM = new AnchorableSnapshotVM(profileService, imagingMediator, cameraMediator, applicationStatusMediator);
                }
                return _anchorableSnapshotVM;
            }
            set {
                _anchorableSnapshotVM = value;
                RaisePropertyChanged();
            }
        }

        //zhang085
        //private PolarAlignmentVM _polarAlignVM;

        //public PolarAlignmentVM PolarAlignVM {
        //    get {
        //        if (_polarAlignVM == null) {
        //            _polarAlignVM = new PolarAlignmentVM(profileService, cameraMediator, imagingMediator, applicationStatusMediator);
        //        }
        //        return _polarAlignVM;
        //    }
        //    set {
        //        _polarAlignVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //zhang080
        //private AnchorablePlateSolverVM _platesolveVM;

        //public AnchorablePlateSolverVM PlatesolveVM {
        //    get {
        //        if (_platesolveVM == null) {
        //            _platesolveVM = new AnchorablePlateSolverVM(profileService, cameraMediator, imagingMediator, applicationStatusMediator);
        //        }
        //        return _platesolveVM;
        //    }
        //    set {
        //        _platesolveVM = value;
        //        RaisePropertyChanged();
        //    }
        //}


        //zhang045
        //private TelescopeVM _telescopeVM;

        //public TelescopeVM TelescopeVM {
        //    get {
        //        if (_telescopeVM == null) {
        //            _telescopeVM = new TelescopeVM(profileService, telescopeMediator, applicationStatusMediator);
        //        }
        //        return _telescopeVM;
        //    }
        //    set {
        //        _telescopeVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //zhang058
        //private GuiderVM _guiderVM;

        //public GuiderVM GuiderVM {
        //    get {
        //        if (_guiderVM == null) {
        //            _guiderVM = new
        //                GuiderVM(profileService, guiderMediator, cameraMediator, applicationStatusMediator);
        //        }
        //        return _guiderVM;
        //    }
        //    set {
        //        _guiderVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        private OptionsVM _optionsVM;

        public OptionsVM OptionsVM {
            get {
                if (_optionsVM == null) {
                    _optionsVM = new OptionsVM(profileService, filterWheelMediator);
                }
                return _optionsVM;
            }
            set {
                _optionsVM = value;
                RaisePropertyChanged();
            }
        }

        //zhang099
        //private FocusTargetsVM focusTargetsVM;

        //public FocusTargetsVM FocusTargetsVM {
        //    get => focusTargetsVM ?? (focusTargetsVM = new FocusTargetsVM(profileService, new ApplicationResourceDictionary()));
        //    set {
        //        focusTargetsVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //zhang101
        //private AutoFocusVM _autoFocusVM;

        //public AutoFocusVM AutoFocusVM {
        //    get {
        //        if (_autoFocusVM == null) {
        //            _autoFocusVM = new AutoFocusVM(profileService, cameraMediator, filterWheelMediator, imagingMediator, applicationStatusMediator);
        //        }
        //        return _autoFocusVM;
        //    }
        //    set {
        //        _autoFocusVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //zhang002
        //private FramingAssistantVM _framingAssistantVM;

        //public FramingAssistantVM FramingAssistantVM {
        //    get {
        //        if (_framingAssistantVM == null) {
        //            _framingAssistantVM = new FramingAssistantVM(profileService, cameraMediator, telescopeMediator, applicationStatusMediator);
        //        }
        //        return _framingAssistantVM;
        //    }
        //    set {
        //        _framingAssistantVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //zhang073
        //private SkyAtlasVM _skyAtlasVM;

        //public SkyAtlasVM SkyAtlasVM {
        //    get {
        //        if (_skyAtlasVM == null) {
        //            _skyAtlasVM = new SkyAtlasVM(profileService);
        //        }
        //        return _skyAtlasVM;
        //    }
        //    set {
        //        _skyAtlasVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //zhang089
        //private ExposureCalculatorVM exposureCalculatorVM;

        //public ExposureCalculatorVM ExposureCalculatorVM {
        //    get {
        //        if (exposureCalculatorVM == null) {
        //            exposureCalculatorVM = new ExposureCalculatorVM(profileService, imagingMediator);
        //        }
        //        return exposureCalculatorVM;
        //    }
        //    set {
        //        exposureCalculatorVM = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public ICommand MinimizeWindowCommand { get; private set; }

        public ICommand MaximizeWindowCommand { get; private set; }
        public ICommand CheckProfileCommand { get; }
        public ICommand CheckUpdateCommand { get; private set; }
        public ICommand OpenManualCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand ClosingCommand { get; private set; }
        public ICommand ConnectAllDevicesCommand { get; private set; }
        public ICommand DisconnectAllDevicesCommand { get; private set; }
    }

    public enum ApplicationTab {
        EQUIPMENT,
        SKYATLAS,
        FRAMINGASSISTANT,
        FLATWIZARD,
        SEQUENCE,
        IMAGING,
        OPTIONS
    }
}