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
using NINA.Profile;
using NINA.Utility.WindowService;
using System;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using NINA.Utility.Notification;
using System.Linq;

namespace NINA.ViewModel {

    internal class ProfileSelectVM : BaseINPC {
        private CancellationTokenSource _cancelTokenSource;
        private IProfile _defaultProfile;
        private IProfile _tempProfile;

        public ProfileSelectVM(IProfileService profileService) {
            this.profileService = profileService;
            Profiles = profileService.Profiles;
            selectedProfileMeta = profileService.Profiles.Where(x => x.Id == profileService.ActiveProfile.Id).First();
            _tempProfile = profileService.ActiveProfile;
            _defaultProfile = ActiveProfile;
        }

        private IProfileService profileService;

        public ICollection<ProfileMeta> Profiles { set; get; }

        private ProfileMeta selectedProfileMeta;

        public ProfileMeta SelectedProfileMeta {
            get => selectedProfileMeta;
            set {
                if (profileService.SelectProfile(value)) {
                    selectedProfileMeta = value;
                    RaisePropertyChanged(nameof(ActiveProfile));
                    RaisePropertyChanged(nameof(Camera));
                    RaisePropertyChanged(nameof(FilterWheel));
                    //zhang096
                    //RaisePropertyChanged(nameof(Telescope));
                    //RaisePropertyChanged(nameof(FocalLength));
                    //RaisePropertyChanged(nameof(Focuser));
                } else {
                    Notification.ShowWarning(Locale.Loc.Instance["LblSelectProfileInUseWarning"]);
                }
                RaisePropertyChanged();
            }
        }

        public IProfile ActiveProfile {
            get {
                return profileService.ActiveProfile;
            }
        }

        public string Camera {
            get {
                return ActiveProfile.CameraSettings.Id;
            }
        }

        public string FilterWheel {
            get {
                return ActiveProfile.FilterWheelSettings.Id;
            }
        }

        //zhang096
        //public string FocalLength {
        //    get {
        //        return ActiveProfile.TelescopeSettings.FocalLength.ToString(CultureInfo.InvariantCulture);
        //    }
        //}

        //zhang107
        //public string Focuser {
        //    get {
        //        return ActiveProfile.FocuserSettings.Id;
        //    }
        //}

        //zhang096
        //public string Telescope {
        //    get {
        //        return ActiveProfile.TelescopeSettings.Id;
        //    }
        //}

        private IWindowServiceFactory windowServiceFactory;

        public IWindowServiceFactory WindowServiceFactory {
            get {
                if (windowServiceFactory == null) {
                    windowServiceFactory = new WindowServiceFactory();
                }
                return windowServiceFactory;
            }
            set {
                windowServiceFactory = value;
            }
        }

        public bool UseSavedProfile { get; set; } = Properties.Settings.Default.UseSavedProfileSelection;

        public void SelectProfile() {
            _cancelTokenSource?.Dispose();
            _cancelTokenSource = new CancellationTokenSource();
            try {
                if (!UseSavedProfile) {
                    var ws = WindowServiceFactory.Create();
                    ws.OnDialogResultChanged += (s, e) => {
                        var dialogResult = (DialogResultEventArgs)e;
                        if (dialogResult.DialogResult != true) {
                            _cancelTokenSource.Cancel();
                            profileService.SelectProfile(new ProfileMeta() { Id = _defaultProfile.Id, Name = _defaultProfile.Name, Location = _defaultProfile.Location });
                        } else {
                            if (UseSavedProfile == true) {
                                Properties.Settings.Default.UseSavedProfileSelection = true;
                                Properties.Settings.Default.Save();
                            }
                        }
                    };
                    ws.ShowDialog(this, Locale.Loc.Instance["LblChooseProfile"], System.Windows.ResizeMode.CanResize, System.Windows.WindowStyle.SingleBorderWindow);
                }
            } catch (OperationCanceledException) {
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }
    }
}