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
using NINA.Utility;
using NINA.Utility.Exceptions;
using NINA.Utility.Mediator.Interfaces;
using NINA.Utility.Notification;
using NINA.Profile;
using NINA.ViewModel.Interfaces;
using System;
using System.Collections.Async;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static NINA.Model.CaptureSequence;
using NINA.Model.ImageData;
using NINA.Model.MyFilterWheel;
using NINA.Utility.Mediator;
using System.Windows.Forms;

namespace NINA.ViewModel {
   
    internal  class ImagingVM : BaseVM, IImagingVM {

        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        //实例化一个值为1的信号量的Singleton。这意味着一次只能授予一个线程访问权限。
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private ImageControlVM _imageControl;

        private Task<IRenderedImage> _imageProcessingTask;

        private ApplicationStatus _status;

        private IApplicationStatusMediator applicationStatusMediator;

        private CameraInfo cameraInfo;

        private ICameraMediator cameraMediator;

        private FilterWheelInfo filterWheelInfo;

        private IFilterWheelMediator filterWheelMediator;

        private IImagingMediator imagingMediator;

        private ImageStatisticsVM imgStatisticsVM;

        private IProgress<ApplicationStatus> progress;

        //zhang096
        //private TelescopeInfo telescopeInfo;


        public ImagingVM(
                IProfileService profileService,
                IImagingMediator imagingMediator,
                ICameraMediator cameraMediator,
                IFilterWheelMediator filterWheelMediator,
                IApplicationStatusMediator applicationStatusMediator
        ) : base(profileService) {
            this.imagingMediator = imagingMediator;
            this.imagingMediator.RegisterHandler(this);

            this.cameraMediator = cameraMediator;
            this.cameraMediator.RegisterConsumer(this);


            this.filterWheelMediator = filterWheelMediator;
            this.filterWheelMediator.RegisterConsumer(this);

            this.applicationStatusMediator = applicationStatusMediator;

            progress = new Progress<ApplicationStatus>(p => Status = p);

            ImageControl = new ImageControlVM(profileService, cameraMediator, applicationStatusMediator);
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

        public ImageControlVM ImageControl {
            get { return _imageControl; }
            set { _imageControl = value; RaisePropertyChanged(); }
        }

        public ImageStatisticsVM ImgStatisticsVM {
            get {
                if (imgStatisticsVM == null) {
                    imgStatisticsVM = new ImageStatisticsVM(profileService);
                }
                return imgStatisticsVM;
            }
            set {
                imgStatisticsVM = value;
                RaisePropertyChanged();
            }
        }

        public ApplicationStatus Status {
            get {
                return _status;
            }
            set {
                _status = value;
                _status.Source = Locale.Loc.Instance["LblImaging"]; ;
                RaisePropertyChanged();

                applicationStatusMediator.StatusUpdate(_status);
            }
        }

        private void AddMetaData(
            ImageMetaData metaData,
            CaptureSequence sequence,
            DateTime start,
            string targetName) {
            metaData.Image.ExposureStart = start;
            metaData.Image.Binning = sequence.Binning.Name;
            metaData.Image.ExposureNumber = sequence.ProgressExposureCount;
            metaData.Image.ExposureTime = sequence.ExposureTime;
            metaData.Image.ImageType = sequence.ImageType;
            metaData.Target.Name = targetName;

            // Fill all available info from profile
            metaData.FromProfile(profileService.ActiveProfile);

            //zhang096
            //metaData.FromTelescopeInfo(telescopeInfo);
            metaData.FromFilterWheelInfo(filterWheelInfo);

            metaData.FilterWheel.Filter = sequence.FilterType?.Name ?? metaData.FilterWheel.Filter;
        }

        private Task<IExposureData> CaptureImage(
                CaptureSequence sequence,//一个序列化的流程，类似于一系列的自动任务。
                PrepareImageParameters parameters,
                CancellationToken token,
                string targetName = "",
                bool skipProcessing = false
                ) {
            return Task.Run(async () => {
                try {
                    IExposureData data = null;
                    //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released
                    progress.Report(new ApplicationStatus() { Status = Locale.Loc.Instance["LblWaitingForCamera"] });
                    await semaphoreSlim.WaitAsync(token);

                    try {
                        if (CameraInfo.Connected != true) {
                            Notification.ShowWarning(Locale.Loc.Instance["LblNoCameraConnected"]);
                            throw new CameraConnectionLostException();
                        }

                        /*Change Filter*///一般天文镜有滤镜轮，这里是转动滤镜轮选择合适的滤镜。学习一下
                        await ChangeFilter(sequence, token, progress);

                        /* Start RMS Recording *///guiderMediator是导星任务相关的东西，不管无用。
                        //zhang061
                        //var rmsHandle = this.guiderMediator.StartRMSRecording();

                        /*Capture*/
                        var exposureStart = DateTime.Now;
                        await cameraMediator.Capture(sequence, token, progress);

                        /* Stop RMS Recording */
                        //zhang061
                        //var rms = this.guiderMediator.StopRMSRecording(rmsHandle);

                        /*Download Image */
                        data = await Download(token, progress);

                        token.ThrowIfCancellationRequested();

                        if (data == null) {
                            Logger.Error(new CameraDownloadFailedException(sequence));
                            Notification.ShowError(string.Format(Locale.Loc.Instance["LblCameraDownloadFailed"], sequence.ExposureTime, sequence.ImageType, sequence.Gain, sequence.FilterType?.Name ?? string.Empty));
                            return null;
                        }

                        AddMetaData(data.MetaData, sequence, exposureStart, targetName);//targetName指拍摄的是月亮还是木星等。rms是赤道仪的一个精度，这个函数不用管。

                        if (!skipProcessing) {
                            //Wait for previous prepare image task to complete
                            if (_imageProcessingTask != null && !_imageProcessingTask.IsCompleted) {
                                progress.Report(new ApplicationStatus() { Status = Locale.Loc.Instance["LblWaitForImageProcessing"] });
                                await _imageProcessingTask;
                            }

                            _imageProcessingTask = PrepareImage(data, parameters, token);
                        }
                    } catch (System.OperationCanceledException ex) {
                        cameraMediator.AbortExposure();
                        throw ex;
                    } catch (CameraConnectionLostException ex) {
                        Logger.Error(ex);
                        Notification.ShowError(Locale.Loc.Instance["LblCameraConnectionLost"]);
                        throw ex;
                    } catch (Exception ex) {
                        Notification.ShowError(Locale.Loc.Instance["LblUnexpectedError"] + Environment.NewLine + ex.Message);
                        Logger.Error(ex);
                        cameraMediator.AbortExposure();
                        throw ex;
                    } finally {
                        progress.Report(new ApplicationStatus() { Status = "" });
                        semaphoreSlim.Release();
                    }
                    return data;
                } finally {
                    progress.Report(new ApplicationStatus() { Status = string.Empty });
                }
            });
        }

        private async Task ChangeFilter(CaptureSequence seq, CancellationToken token, IProgress<ApplicationStatus> progress) {
            if (seq.FilterType != null) {
                await filterWheelMediator.ChangeFilter(seq.FilterType, token, progress);
            }
        }

        private Task<IExposureData> Download(CancellationToken token, IProgress<ApplicationStatus> progress) {
            progress.Report(new ApplicationStatus() { Status = Locale.Loc.Instance["LblDownloading"] });
            return cameraMediator.Download(token);
        }

        public async Task<IRenderedImage> CaptureAndPrepareImage(
            CaptureSequence sequence,
            PrepareImageParameters parameters,
            CancellationToken token,
            IProgress<ApplicationStatus> progress) {
            var iarr = await CaptureImage(sequence, parameters, token, string.Empty);
            if (iarr != null) {
                return await _imageProcessingTask;
            } else {
                return null;
            }
        }

        public Task<IExposureData> CaptureImage(CaptureSequence sequence, CancellationToken token, IProgress<ApplicationStatus> progress) {
            return CaptureImage(sequence, new PrepareImageParameters(), token, string.Empty, true);
        }

        public void DestroyImage() {
            ImageControl.Image = null;
            ImageControl.RenderedImage = null;
        }

        public void Dispose() {
            this.cameraMediator.RemoveConsumer(this);
            this.filterWheelMediator.RemoveConsumer(this);
        }

        public Task<IRenderedImage> PrepareImage(
            IExposureData data,
            PrepareImageParameters parameters,
            CancellationToken cancelToken) {
            _imageProcessingTask = Task.Run(async () => {
                var imageData = await data.ToImageData();
                var processedData = await ImageControl.PrepareImage(imageData, parameters, cancelToken);
                await ImgStatisticsVM.UpdateStatistics(imageData);//统计星点大小数据，不用管
                return processedData;
            }, cancelToken);
            return _imageProcessingTask;
        }

        public Task<IRenderedImage> PrepareImage(
            IImageData data,
            PrepareImageParameters parameters,
            CancellationToken cancelToken) {
            _imageProcessingTask = Task.Run(async () => {
                var processedData = await ImageControl.PrepareImage(data, parameters, cancelToken);
                await ImgStatisticsVM.UpdateStatistics(data);
                return processedData;
            }, cancelToken);
            return _imageProcessingTask;
        }

        public void SetImage(BitmapSource img) {
            ImageControl.Image = img;
        }

        public async Task<bool> StartLiveView(CancellationToken ct) {
            ImageControl.IsLiveViewEnabled = true;
            try {
                var liveViewEnumerable = cameraMediator.LiveView(ct);
                await liveViewEnumerable.ForEachAsync(async exposureData => {
                    var imageData = await exposureData.ToImageData(ct);
                    await ImageControl.PrepareImage(imageData, new PrepareImageParameters(), ct);
                });
            } catch (OperationCanceledException) {
            } finally {
                ImageControl.IsLiveViewEnabled = false;
            }

            return true;
        }

        //显示文件图片：

        public async Task<bool> StartFileView(string filePath ,CancellationToken ct)
        {
            ImageControl.IsLiveViewEnabled = true;
            try
            {   
                var filecam = new FileCamera(profileService);
 
                var exposureData = await filecam.DownloadExposureFromFile(filePath, ct);

                var imageData = await exposureData.ToImageData(ct);
               
                await ImageControl.PrepareImage(imageData, new PrepareImageParameters(), ct);

                await ImgStatisticsVM.UpdateStatistics(imageData);


            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                ImageControl.IsLiveViewEnabled = false;
            }

            return true;
        }

            public void UpdateDeviceInfo(CameraInfo cameraStatus) {
            CameraInfo = cameraStatus;
        }

        //zhang096
        //public void UpdateDeviceInfo(TelescopeInfo deviceInfo) {
        //    this.telescopeInfo = deviceInfo;
        //}

        public void UpdateDeviceInfo(FilterWheelInfo deviceInfo) {
            this.filterWheelInfo = deviceInfo;
        }

    }
}