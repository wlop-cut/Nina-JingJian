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

using NINA.Model;
using NINA.Model.MyCamera;
using NINA.Profile;
using NINA.Utility;
using NINA.Utility.Mediator;
using NINA.Utility.Mediator.Interfaces;
using NINA.Utility.Notification;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using static NINA.Model.CaptureSequence;
using System.Diagnostics;
using System.IO;

namespace NINA.ViewModel.Imaging {

    internal class AnchorableSnapshotVM : DockableVM, ICameraConsumer {
        private CancellationTokenSource _captureImageToken;
        private CancellationTokenSource _liveViewCts;
        private bool _liveViewEnabled;
        private BinningMode _snapBin;
        private bool _snapSubSample;

        private ApplicationStatus _status;

        private IApplicationStatusMediator applicationStatusMediator;
        private CameraInfo cameraInfo;
        private ICameraMediator cameraMediator;
        private IImagingMediator imagingMediator;

        private IProgress<ApplicationStatus> progress;
        public AnchorableSnapshotVM(
                IProfileService profileService,//配置文件服务
                IImagingMediator imagingMediator,//图像调解器
                ICameraMediator cameraMediator,//相机调解器
                IApplicationStatusMediator applicationStatusMediator) : base(profileService) {//应用程序状态调解器
            Title = "LblImaging";
            ImageGeometry = (System.Windows.Media.GeometryGroup)System.Windows.Application.Current.Resources["ImagingSVG"];
            this.applicationStatusMediator = applicationStatusMediator;
            this.cameraMediator = cameraMediator;
            this.cameraMediator.RegisterConsumer(this);
            this.imagingMediator = imagingMediator;
            progress = new Progress<ApplicationStatus>(p => Status = p);
            SnapCommand = new AsyncCommand<bool>(() => SnapImage(progress));
            CancelSnapCommand = new RelayCommand(CancelSnapImage);
            StartLiveViewCommand = new AsyncCommand<bool>(StartLiveView);
            StopLiveViewCommand = new RelayCommand(StopLiveView);
        }

        /// <summary>
        /// Backwards compatible ContentId due to refactoring
        /// 由于重构而向后兼容的 ContentId
        /// </summary>
        public new string ContentId {
            get => typeof(ImagingVM).Name;
        }

        public CameraInfo CameraInfo {
            get {
                return cameraInfo ?? DeviceInfo.CreateDefaultInstance<CameraInfo>();
            }
            set {
                cameraInfo = value;
                RaisePropertyChanged();
            }
        }

        public ICommand CancelSnapCommand { get; private set; }

        private bool isLooping;

        public bool IsLooping {
            get => isLooping;
            set {
                isLooping = value;
                RaisePropertyChanged();
            }
        }

        public bool LiveViewEnabled {
            get {
                return _liveViewEnabled;
            }
            set {
                _liveViewEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool Loop {
            get {
                return profileService.ActiveProfile.SnapShotControlSettings.Loop;
            }
            set {
                profileService.ActiveProfile.SnapShotControlSettings.Loop = value;
                RaisePropertyChanged();
            }
        }

        public BinningMode SnapBin {
            get {
                if (_snapBin == null) {
                    _snapBin = new BinningMode(1, 1);
                }
                return _snapBin;
            }
            set {
                _snapBin = value;
                RaisePropertyChanged();
            }
        }

        public IAsyncCommand SnapCommand { get; private set; }

        public double SnapExposureDuration {
            get {
                return profileService.ActiveProfile.SnapShotControlSettings.ExposureDuration;
            }

            set {
                profileService.ActiveProfile.SnapShotControlSettings.ExposureDuration = value;
                RaisePropertyChanged();
            }
        }

        public Model.MyFilterWheel.FilterInfo SnapFilter {
            get {
                return profileService.ActiveProfile.SnapShotControlSettings.Filter;
            }
            set {
                profileService.ActiveProfile.SnapShotControlSettings.Filter = value;
                RaisePropertyChanged();
            }
        }

        public int SnapGain {
            get {
                return profileService.ActiveProfile.SnapShotControlSettings.Gain;
            }
            set {
                profileService.ActiveProfile.SnapShotControlSettings.Gain = value;
                RaisePropertyChanged();
            }
        }

        public bool SnapSave {
            get {
                return profileService.ActiveProfile.SnapShotControlSettings.Save;
            }
            set {
                profileService.ActiveProfile.SnapShotControlSettings.Save = value;
                RaisePropertyChanged();
            }
        }

        //增加一个方法：从PHD2获取图片
        //zhang095
        //public bool SnapPHD2
        //{
        //    get
        //    {
        //        return profileService.ActiveProfile.SnapShotControlSettings.SnapPHD2;
        //    }
        //    set
        //    {
        //        profileService.ActiveProfile.SnapShotControlSettings.SnapPHD2 = value;
        //        RaisePropertyChanged();
        //    }
        //}


        public bool SnapSubSample {
            get {
                return _snapSubSample;
            }
            set {
                _snapSubSample = value;
                RaisePropertyChanged();
            }
        }

        public IAsyncCommand StartLiveViewCommand { get; private set; }//private set等价于一个只读属性，只能在该类的构造函数中赋值

        public ApplicationStatus Status {
            get {
                return _status;
            }
            set {
                _status = value;
                _status.Source = Title;
                RaisePropertyChanged();

                applicationStatusMediator.StatusUpdate(_status);
            }
        }

        public ICommand StopLiveViewCommand { get; private set; }

        private void CancelSnapImage(object o) {
            _captureImageToken?.Cancel();
        }

        private async Task<bool> StartLiveView() {
            _liveViewCts?.Dispose();//释放资源，即释放已分配的内存
            _liveViewCts = new CancellationTokenSource();
            try {
                await this.imagingMediator.StartLiveView(_liveViewCts.Token);
            } catch (OperationCanceledException) {
            }

            return true;
        }

        private void StopLiveView(object o) {
            _liveViewCts?.Cancel();
        }

        public void Dispose() {
            this.cameraMediator.RemoveConsumer(this);
        }

        public async Task<bool> SnapImage(IProgress<ApplicationStatus> progress)
        {
            _captureImageToken?.Dispose();
            _captureImageToken = new CancellationTokenSource();
            string ImagePath = null;
                        
            try {
                var success = true;
                if (Loop) IsLooping = true;              
                do {
                    //if (!SnapPHD2)
                    //{
                        var seq = new CaptureSequence(SnapExposureDuration, ImageTypes.SNAPSHOT, SnapFilter, SnapBin, 1);//捕获序列
                        seq.EnableSubSample = SnapSubSample;
                        seq.Gain = SnapGain;//经过前面的步骤，已经获取到rawimage,即相机的电荷图。后续要通过算法对图像进行提取，否则只是黑漆漆一片。

                        var renderedImage = await imagingMediator.CaptureAndPrepareImage(seq, new PrepareImageParameters(), _captureImageToken.Token, progress);
                        //   渲染图像             成像调解器        捕获并准备图像   
                        if (SnapSave)
                        {
                            var path = await renderedImage.RawImageData.SaveToDisk(new FileSaveInfo(profileService), _captureImageToken.Token);
                            var imageStatistics = await renderedImage.RawImageData.Statistics.Task;

                            imagingMediator.OnImageSaved(new ImageSavedEventArgs()
                            {
                                    PathToImage = new Uri(path),
                                    Image = renderedImage.Image,
                                    FileType = profileService.ActiveProfile.ImageFileSettings.FileType,
                                    Mean = imageStatistics.Mean,
                                    //zhang105
                                    //HFR = renderedImage.RawImageData.StarDetectionAnalysis.HFR,
                                    Duration = renderedImage.RawImageData.MetaData.Image.ExposureTime,
                                    IsBayered = renderedImage.RawImageData.Properties.IsBayered,
                                    Filter = renderedImage.RawImageData.MetaData.FilterWheel.Filter
                            });
                        } 
                    //}

                    //else
                    //{
                    //    PHD2Guider guider = new PHD2Guider(profileService);

                    //    if (Process.GetProcessesByName("phd2").Length == 0)
                    //    {
                    //        progress.Report(new ApplicationStatus() { Status = Locale.Loc.Instance["LblWaitingForPHD2"] });
                    //        await guider.Connect();                                                                     
                    //    }
                    //    if (Process.GetProcessesByName("phd2").Length != 0)

                    //    {
                    //        string ss = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\phd2";
                    //        if (Directory.Exists(ss))
                    //        {
                    //            if (Directory.GetFiles(ss).Length > 0)
                    //            {
                    //                foreach (string var in Directory.GetFiles(ss))
                    //                {
                    //                    File.Delete(var);
                    //                }
                    //            }
                    //        }
                    //        progress.Report(new ApplicationStatus() { Status = Locale.Loc.Instance["LblWaitingForPHDImaging"] });

                    //        await guider.ConnectPHD2Equipment();

                    //        double snaptime = SnapExposureDuration * 1000;   //将时间单位s转换为ms来发送

                    //        Guider phdguider = Guider.Factory(profileService.ActiveProfile.GuiderSettings.PHD2ServerUrl);

                    //        var guiderresult = phdguider.CaptureandSave(profileService.ActiveProfile.GuiderSettings.PHD2ServerUrl, (int)snaptime);

                    //        var GuiderWidth = guiderresult["Size"][0];
                    //        var GuiderHeight = guiderresult["Size"][1];
                    //        var PixelScale = (double)guiderresult["PixelScale"];

                    //        string ImagePath1 = guiderresult["Path"].ToString();
                    //        if (ImagePath1.IndexOf(".tmp") != -1)
                    //        {
                    //            ImagePath = ImagePath1.Replace(".tmp", ".fit");
                    //        }

                    //        if (System.IO.File.Exists(ImagePath1))
                    //        {
                    //            System.IO.Directory.Move(ImagePath1, ImagePath);
                    //        }
                    //        if (File.Exists(ImagePath))
                    //        {
                    //            string imageDir = Path.GetDirectoryName(ImagePath);
                    //            string iamgeName = Path.GetFileName(ImagePath);
                    //            var files = Directory.GetFiles(imageDir);
                    //            if (files != null)
                    //            {
                    //                foreach (var file in files)
                    //                {
                    //                    if (file != ImagePath)
                    //                    {
                    //                        File.Delete(file);
                    //                    }
                    //                }
                    //            }
                    //        }
                           
                    //        //这里如果实现，就可以显示图像。 mjnb

                    //        var dispaly = await this.imagingMediator.StartFileView(ImagePath, _captureImageToken.Token);

                    //        if (_captureImageToken.Token.IsCancellationRequested)
                    //        {
                    //            phdguider.StopCapture();
                    //        }
                    //        if (SnapSave)
                    //        {
                    //            if (System.IO.File.Exists(ImagePath))
                    //            {
                    //                string filename_ ="PHD2" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".fit";
                    //                FileInfo fileold = new FileInfo(ImagePath);
                    //                fileold.CopyTo(Path.Combine(profileService.ActiveProfile.ImageFileSettings.FilePath, filename_));                                    
                    //            }
                    //        }
                    //    }
                    //}                                       
                    _captureImageToken.Token.ThrowIfCancellationRequested();
                } while (Loop && success);

               if (System.IO.File.Exists(ImagePath))
                {
                    System.IO.File.Delete(ImagePath);
                }
            } catch (OperationCanceledException) {
            } catch (Exception ex) {
                Logger.Error(ex);
                Notification.ShowError(ex.Message);
            } finally {
                //if (_imageProcessingTask != null) {
                //    await _imageProcessingTask;
                //}
                IsLooping = false;
                progress.Report(new ApplicationStatus() { Status = string.Empty });
            }
            return true;
        }

        public void UpdateDeviceInfo(CameraInfo cameraStatus) {
            CameraInfo = cameraStatus;
        }
    }
}