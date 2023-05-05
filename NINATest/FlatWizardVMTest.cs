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

using System;
using FluentAssertions;
using Moq;
using NINA.Locale;
using NINA.Model.MyCamera;
using NINA.Model.MyFilterWheel;
using NINA.Utility;
using NINA.Utility.Mediator.Interfaces;
using NINA.Profile;
using NINA.ViewModel.FlatWizard;
using NINA.ViewModel.Interfaces;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NINA.Model;
using NINA.Model.ImageData;
using NINA.Model.MyCamera.Simulator;
using NINA.Model.MyFlatDevice;
using NINA.Utility.Mediator;
using NINA.ViewModel.Equipment.FlatDevice;

namespace NINATest {

    [TestFixture]
    public class FlatWizardVMTest {
        private IFlatWizardVM sut;

        private Mock<IProfileService> profileServiceMock;
        private Mock<IImagingVM> imagingVMMock;
        private Mock<ICameraMediator> cameraMediatorMock;
        private Mock<ITelescopeMediator> telescopeMediatorMock;
        private Mock<IFilterWheelMediator> filterWheelMediatorMock;
        private Mock<IApplicationStatusMediator> applicationStatusMediatorMock;
        private Mock<IFlatWizardExposureTimeFinderService> exposureServiceMock;
        private Mock<ILoc> localeMock;
        private Mock<IApplicationResourceDictionary> resourceDictionaryMock;
        private Mock<IProfile> profileMock;
        private Mock<IFlatWizardSettings> flatWizardSettingsMock;
        private Mock<ICameraSettings> cameraSettingsMock;
        private Mock<IFilterWheelSettings> filterWheelSettingsMock;
        private Mock<IFlatDeviceMediator> _flatDeviceMediatorMock;
        private SimulatorCamera _camera;
        private IRenderedImage _image;
        private FlatDeviceInfo _flatDevice;

        [SetUp]
        public void Init() {
            filterWheelMediatorMock = new Mock<IFilterWheelMediator>();
            profileServiceMock = new Mock<IProfileService>();
            imagingVMMock = new Mock<IImagingVM>();
            cameraMediatorMock = new Mock<ICameraMediator>();
            telescopeMediatorMock = new Mock<ITelescopeMediator>();
            applicationStatusMediatorMock = new Mock<IApplicationStatusMediator>();
            exposureServiceMock = new Mock<IFlatWizardExposureTimeFinderService>();
            localeMock = new Mock<ILoc>();
            filterWheelSettingsMock = new Mock<IFilterWheelSettings>();
            _flatDeviceMediatorMock = new Mock<IFlatDeviceMediator>();
            profileMock = new Mock<IProfile>();
            flatWizardSettingsMock = new Mock<IFlatWizardSettings>();
            cameraSettingsMock = new Mock<ICameraSettings>();
            resourceDictionaryMock = new Mock<IApplicationResourceDictionary>();
            profileServiceMock.SetupGet(m => m.ActiveProfile).Returns(profileMock.Object);
            profileMock.SetupGet(m => m.FlatWizardSettings).Returns(flatWizardSettingsMock.Object);
            profileMock.SetupGet(m => m.CameraSettings).Returns(cameraSettingsMock.Object);
            profileMock.SetupGet(m => m.FilterWheelSettings).Returns(filterWheelSettingsMock.Object);
            profileServiceMock.Setup(m => m.ActiveProfile.ImageSettings.AutoStretchFactor).Returns(1.0);
            profileServiceMock.Setup(m => m.ActiveProfile.ImageSettings.BlackClipping).Returns(0.1);
            filterWheelSettingsMock.SetupGet(m => m.FilterWheelFilters)
                .Returns(new ObserveAllCollection<FilterInfo>());

            _camera = new SimulatorCamera(profileServiceMock.Object, telescopeMediatorMock.Object) {
                Settings = { Type = CameraType.RANDOM, RandomSettings = { ImageMean = 32000, ImageStdDev = 1000 } }
            };
            _image = _camera.DownloadExposure(new CancellationToken()).Result.ToImageData().Result.RenderImage();
            localeMock.SetupGet(m => m[It.IsAny<string>()]).Returns("Test");
            imagingVMMock.Setup(m => m.CaptureAndPrepareImage(It.IsAny<CaptureSequence>(),
                It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(),
                It.IsAny<IProgress<ApplicationStatus>>())).ReturnsAsync(_image);
            _flatDevice = new FlatDeviceInfo() {
                Brightness = 1.0,
                Connected = true,
                CoverState = CoverState.Open,
                Description = "Some description",
                DriverInfo = "Some driverInfo",
                LightOn = false,
                DriverVersion = "200",
                MaxBrightness = 255,
                MinBrightness = 0,
                Name = "Some name"
            };
        }

        [TearDown]
        public void Dispose() {
            _camera.Dispose();
        }

        [Test]
        public void Constructor_WhenInitialized_DoAllNecessaryCallsAndVerifyData() {
            var filters = new List<FilterInfo>() {
                new FilterInfo("Filter", 0, 0), new FilterInfo("Filter2", 0, 0)
            };
            filterWheelSettingsMock.SetupGet(m => m.FilterWheelFilters)
                .Returns(new ObserveAllCollection<FilterInfo>(filters));

            // act

            sut = new FlatWizardVM(profileServiceMock.Object, imagingVMMock.Object,
                cameraMediatorMock.Object, filterWheelMediatorMock.Object, telescopeMediatorMock.Object,
                _flatDeviceMediatorMock.Object, resourceDictionaryMock.Object,
                applicationStatusMediatorMock.Object) {
                FlatWizardExposureTimeFinderService = exposureServiceMock.Object,
                Locale = localeMock.Object,
            };

            // assert
            sut.ImagingVM.Should().NotBeNull();

            sut.StartFlatSequenceCommand.Should().NotBeNull();
            sut.CancelFlatExposureSequenceCommand.Should().NotBeNull();
            sut.PauseFlatExposureSequenceCommand.Should().NotBeNull();
            sut.ResumeFlatExposureSequenceCommand.Should().NotBeNull();

            sut.SingleFlatWizardFilterSettings.Should().NotBeNull();
            sut.SingleFlatWizardFilterSettings.Filter.Should().BeNull();

            flatWizardSettingsMock.Verify(m => m.HistogramMeanTarget, Times.Once);
            flatWizardSettingsMock.Verify(m => m.HistogramTolerance, Times.Once);
            flatWizardSettingsMock.Verify(m => m.StepSize, Times.Once);
            cameraSettingsMock.Verify(m => m.MaxFlatExposureTime, Times.Once);
            cameraSettingsMock.Verify(m => m.MinFlatExposureTime, Times.Once);

            flatWizardSettingsMock.Verify(m => m.FlatCount, Times.AtMost(2));
            flatWizardSettingsMock.Verify(m => m.BinningMode, Times.AtMost(2));

            filterWheelSettingsMock.Verify(m => m.FilterWheelFilters, Times.AtMost(2));

            cameraMediatorMock.Verify(m => m.RegisterConsumer(sut), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.RegisterConsumer(sut), Times.Once);

            sut.Filters.Select(f => f.Filter).Should().BeEquivalentTo(filters);
        }

        [Test]
        public void UpdateDeviceInfo_WhenCalled_SetVariablesAndCameraInfoInAllFilters() {
            filterWheelSettingsMock.SetupGet(m => m.FilterWheelFilters)
                .Returns(new ObserveAllCollection<FilterInfo>() {
                    new FilterInfo("Filter", 0, 0), new FilterInfo("Filter2", 0, 0)
                });
            // setup
            sut = new FlatWizardVM(profileServiceMock.Object, imagingVMMock.Object,
                cameraMediatorMock.Object, filterWheelMediatorMock.Object,
                telescopeMediatorMock.Object, _flatDeviceMediatorMock.Object,
                resourceDictionaryMock.Object, applicationStatusMediatorMock.Object) {
                FlatWizardExposureTimeFinderService = exposureServiceMock.Object,
                Locale = localeMock.Object,
            };

            var cameraInfo = new CameraInfo {
                Connected = true,
                BitDepth = 16
            };

            // act
            sut.UpdateDeviceInfo(cameraInfo);

            // assert

            sut.CameraConnected.Should().BeTrue();
            sut.SingleFlatWizardFilterSettings.BitDepth.Should().Be(cameraInfo.BitDepth);
            sut.Filters.Select(m => m.BitDepth).Should().AllBeEquivalentTo(cameraInfo.BitDepth);
        }

        [Test]
        public void UpdateFilterWheelSettings_WhenCalled_UpdateFiltersListAndKeepSelectedFilter() {
            // setup
            var filters = new ObserveAllCollection<FilterInfo>() { new FilterInfo("Filter", 0, 0), new FilterInfo("Filter2", 0, 0) };
            filterWheelSettingsMock.SetupGet(m => m.FilterWheelFilters)
                .Returns(filters);

            sut = new FlatWizardVM(profileServiceMock.Object, imagingVMMock.Object,
                cameraMediatorMock.Object, filterWheelMediatorMock.Object, telescopeMediatorMock.Object,
                _flatDeviceMediatorMock.Object, resourceDictionaryMock.Object,
                applicationStatusMediatorMock.Object) {
                FlatWizardExposureTimeFinderService = exposureServiceMock.Object,
                Locale = localeMock.Object,
            };

            var selectedFilter = filters[0];
            sut.SelectedFilter = selectedFilter;

            var cameraInfo = new CameraInfo {
                Connected = true,
                BitDepth = 16
            };

            // pre-assert
            sut.Filters.Select(f => f.Filter).Should().BeEquivalentTo(filters);
            sut.SelectedFilter.Should().BeEquivalentTo(selectedFilter);

            // remove one filter
            filters.RemoveAt(1);

            // update the camera info on existing filters
            sut.UpdateDeviceInfo(cameraInfo);

            // assert cameraInfo on all filters and unused filters are removed
            sut.Filters.Select(f => f.Filter).Should().BeEquivalentTo(filters);
            sut.Filters.Select(f => f.BitDepth).Should().AllBeEquivalentTo(cameraInfo.BitDepth);
            sut.SelectedFilter.Should().BeEquivalentTo(selectedFilter);

            // add another filter
            filters.Add(new FilterInfo("Filter2", 0, 0));
            filters.Add(new FilterInfo("Filter3", 0, 0));

            // assert cameraInfo still on all filters even on the ones that were added
            sut.Filters.Select(f => f.Filter).Should().BeEquivalentTo(filters);
            sut.Filters.Select(f => f.BitDepth).Should().AllBeEquivalentTo(cameraInfo.BitDepth);
            sut.SelectedFilter.Should().BeEquivalentTo(selectedFilter);
        }

        [Test]
        public void UpdateFlatDeviceSettingsAndCheckFlatMagicWithNullFlatDevice() {
            sut = new FlatWizardVM(profileServiceMock.Object, imagingVMMock.Object,
                cameraMediatorMock.Object, filterWheelMediatorMock.Object, telescopeMediatorMock.Object,
                _flatDeviceMediatorMock.Object, resourceDictionaryMock.Object,
                applicationStatusMediatorMock.Object) {
                FlatWizardExposureTimeFinderService = exposureServiceMock.Object,
                Locale = localeMock.Object,
            };

            sut.StartFlatSequenceCommand.Execute(new object());
            _flatDeviceMediatorMock.Verify(m => m.CloseCover(), Times.Never);
            _flatDeviceMediatorMock.Verify(m => m.OpenCover(), Times.Never);
        }

        [Test]
        public async Task UpdateFlatDeviceSettingsAndCheckFlatMagicWithFlatDeviceNoDarkFlats() {
            sut = new FlatWizardVM(profileServiceMock.Object, imagingVMMock.Object,
                cameraMediatorMock.Object, filterWheelMediatorMock.Object, telescopeMediatorMock.Object,
                _flatDeviceMediatorMock.Object, resourceDictionaryMock.Object,
                applicationStatusMediatorMock.Object) {
                FlatWizardExposureTimeFinderService = exposureServiceMock.Object,
                Locale = localeMock.Object,
            };

            _flatDevice.Connected = true;
            _flatDevice.SupportsOpenClose = true;
            sut.UpdateDeviceInfo(_flatDevice);
            await sut.StartFlatSequenceCommand.ExecuteAsync(new object());
            _flatDeviceMediatorMock.Verify(m => m.CloseCover(), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.ToggleLight((object)true), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.SetBrightness(1.0), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.ToggleLight((object)false), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.OpenCover(), Times.Never);
        }

        [Test]
        public async Task UpdateFlatDeviceSettingsAndCheckFlatMagicWithFlatDeviceWithDarkFlatsCoverOpen() {
            sut = new FlatWizardVM(profileServiceMock.Object, imagingVMMock.Object,
                cameraMediatorMock.Object, filterWheelMediatorMock.Object, telescopeMediatorMock.Object,
                _flatDeviceMediatorMock.Object, resourceDictionaryMock.Object,
                applicationStatusMediatorMock.Object) {
                FlatWizardExposureTimeFinderService = exposureServiceMock.Object,
                Locale = localeMock.Object,
                DarkFlatCount = 1
            };

            _flatDevice.Connected = true;
            _flatDevice.SupportsOpenClose = true;
            sut.UpdateDeviceInfo(_flatDevice);
            profileServiceMock.SetupProperty(m => m.ActiveProfile.FlatDeviceSettings.OpenForDarkFlats, true);
            await sut.StartFlatSequenceCommand.ExecuteAsync(new object());
            _flatDeviceMediatorMock.Verify(m => m.CloseCover(), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.ToggleLight((object)true), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.SetBrightness(1.0), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.ToggleLight((object)false), Times.Exactly(2));
            _flatDeviceMediatorMock.Verify(m => m.OpenCover(), Times.Once);
        }

        [Test]
        public async Task UpdateFlatDeviceSettingsAndCheckFlatMagicWithFlatDeviceWithDarkFlatsCoverClosed() {
            sut = new FlatWizardVM(profileServiceMock.Object, imagingVMMock.Object,
                cameraMediatorMock.Object, filterWheelMediatorMock.Object, telescopeMediatorMock.Object,
                _flatDeviceMediatorMock.Object, resourceDictionaryMock.Object,
                applicationStatusMediatorMock.Object) {
                FlatWizardExposureTimeFinderService = exposureServiceMock.Object,
                Locale = localeMock.Object,
                DarkFlatCount = 1
            };

            _flatDevice.Connected = true;
            _flatDevice.SupportsOpenClose = true;
            sut.UpdateDeviceInfo(_flatDevice);
            profileServiceMock.SetupProperty(m => m.ActiveProfile.FlatDeviceSettings.OpenForDarkFlats, false);
            await sut.StartFlatSequenceCommand.ExecuteAsync(new object());
            _flatDeviceMediatorMock.Verify(m => m.CloseCover(), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.ToggleLight((object)true), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.SetBrightness(1.0), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.ToggleLight((object)false), Times.Exactly(2));
            _flatDeviceMediatorMock.Verify(m => m.OpenCover(), Times.Never);
        }

        [Test]
        public async Task UpdateFlatDeviceSettingsAndCheckFlatMagicWithFlatDeviceThatDoesNotOpenCloseAsync() {
            sut = new FlatWizardVM(profileServiceMock.Object, imagingVMMock.Object,
                cameraMediatorMock.Object, filterWheelMediatorMock.Object, telescopeMediatorMock.Object,
                _flatDeviceMediatorMock.Object, resourceDictionaryMock.Object,
                applicationStatusMediatorMock.Object) {
                FlatWizardExposureTimeFinderService = exposureServiceMock.Object,
                Locale = localeMock.Object,
            };

            _flatDevice.Connected = true;
            _flatDevice.SupportsOpenClose = false;
            sut.UpdateDeviceInfo(_flatDevice);
            await sut.StartFlatSequenceCommand.ExecuteAsync(new object());
            _flatDeviceMediatorMock.Verify(m => m.CloseCover(), Times.Never);
            _flatDeviceMediatorMock.Verify(m => m.ToggleLight((object)true), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.SetBrightness(1.0), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.ToggleLight((object)false), Times.Once);
            _flatDeviceMediatorMock.Verify(m => m.OpenCover(), Times.Never);
        }
    }
}