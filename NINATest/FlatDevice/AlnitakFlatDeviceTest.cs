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
using Moq;
using NINA.Model.MyFlatDevice;
using NINA.Profile;
using NINA.Utility.FlatDeviceSDKs.AlnitakSDK;
using NINA.Utility.SerialCommunication;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace NINATest.FlatDevice {

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class AlnitakFlatDeviceTest {

        private sealed class TestScope : IDisposable {
            public AlnitakFlatDevice Sut { get; }
            public Mock<IProfileService> MockProfileService { get; }
            public Mock<IAlnitakDevice> MockSdk { get; }

            public TestScope() {
                MockProfileService = new Mock<IProfileService>();
                MockProfileService.SetupProperty(m => m.ActiveProfile.FlatDeviceSettings.PortName, "COM3");
                MockSdk = new Mock<IAlnitakDevice>();
                MockSdk.Setup(m => m.InitializeSerialPort(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.FromResult(true));
                MockSdk.Setup(m => m.SendCommand<PingResponse>(It.IsAny<PingCommand>()))
                    .Returns(new PingResponse { DeviceResponse = "*P99000" });
                MockSdk.Setup(m => m.SendCommand<StateResponse>(It.IsAny<StateCommand>()))
                    .Returns(new StateResponse { DeviceResponse = "*S99000" });
                MockSdk.Setup(m => m.SendCommand<FirmwareVersionResponse>(It.IsAny<FirmwareVersionCommand>()))
                    .Returns(new FirmwareVersionResponse { DeviceResponse = "*V99124" });
                Sut = new AlnitakFlatDevice(MockProfileService.Object) { Sdk = MockSdk.Object };
            }

            public void Dispose() {
                Sut.Disconnect();
            }
        }

        [Test]
        [TestCase(CoverState.NeitherOpenNorClosed, "*S99000")]
        [TestCase(CoverState.Closed, "*S99001")]
        [TestCase(CoverState.Open, "*S99002")]
        [TestCase(CoverState.Unknown, "*S99003")]
        [TestCase(CoverState.Unknown, "garbage")]
        [TestCase(CoverState.Unknown, null)]
        public async Task TestCoverState(CoverState coverState, string deviceResponse) {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);
                scope.MockSdk.Setup(m => m.SendCommand<StateResponse>(It.IsAny<StateCommand>()))
                    .Returns(new StateResponse { DeviceResponse = deviceResponse });
                Assert.That(scope.Sut.CoverState, Is.EqualTo(coverState));
            }
        }

        [Test]
        [TestCase(CoverState.Unknown, "*S99000")]
        [TestCase(CoverState.Unknown, "garbage")]
        [TestCase(CoverState.Unknown, null)]
        public void TestCoverStateDisconnected(CoverState coverState, string deviceResponse) {
            using (var scope = new TestScope()) {
                scope.MockSdk.Setup(m => m.SendCommand<StateResponse>(It.IsAny<StateCommand>()))
                    .Returns(new StateResponse { DeviceResponse = deviceResponse });
                Assert.That(scope.Sut.CoverState, Is.EqualTo(coverState));
            }
        }

        [Test]
        public async Task TestMinBrightness() {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);
                Assert.That(scope.Sut.MinBrightness, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task TestMaxBrightness() {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);
                Assert.That(scope.Sut.MaxBrightness, Is.EqualTo(255));
            }
        }

        [Test]
        [TestCase(0.0, "*J99000")]
        [TestCase(1.0, "*J99255")]
        [TestCase(0.5, "*J99128")]
        [TestCase(0.0, "garbage")]
        [TestCase(0.0, null)]
        public async Task TestGetBrightness(double brightness, string deviceResponse) {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);

                scope.MockSdk.Setup(m => m.SendCommand<GetBrightnessResponse>(It.IsAny<GetBrightnessCommand>()))
                    .Returns(new GetBrightnessResponse { DeviceResponse = deviceResponse });
                Assert.That(scope.Sut.Brightness, Is.EqualTo(brightness));
            }
        }

        [Test]
        [TestCase(0, "*J99000")]
        [TestCase(0, "*J99255")]
        [TestCase(0, "*J99099")]
        [TestCase(0, "garbage")]
        [TestCase(0, null)]
        public void TestGetBrightnessDisconnected(double brightness, string deviceResponse) {
            using (var scope = new TestScope()) {
                scope.MockSdk.Setup(m => m.SendCommand<GetBrightnessResponse>(It.IsAny<GetBrightnessCommand>()))
                    .Returns(new GetBrightnessResponse { DeviceResponse = deviceResponse });
                Assert.That(scope.Sut.Brightness, Is.EqualTo(brightness));
            }
        }

        [Test]
        [TestCase(0.0, ">B000\r", "*B99000")]
        [TestCase(1.0, ">B255\r", "*B99255")]
        [TestCase(0.5, ">B128\r", "*B99128")]
        [TestCase(-1.0, ">B000\r", "*B99000")]
        [TestCase(2.0, ">B255\r", "*B99255")]
        public async Task TestSetBrightness(double brightness, string command, string response) {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);
                string actual = null;

                scope.MockSdk.Setup(m => m.SendCommand<SetBrightnessResponse>(It.IsAny<SetBrightnessCommand>()))
                    .Callback<ICommand>(arg => actual = arg.CommandString)
                    .Returns(new SetBrightnessResponse { DeviceResponse = response });
                scope.Sut.Brightness = brightness;
                Assert.That(actual, Is.EqualTo(command));
            }
        }

        [Test]
        [TestCase(0, null)]
        [TestCase(255, null)]
        [TestCase(99, null)]
        [TestCase(50, null)]
        [TestCase(-1, null)]
        [TestCase(256, null)]
        public void TestSetBrightnessDisconnected(int brightness, string command) {
            using (var scope = new TestScope()) {
                string actual = null;
                scope.MockSdk.Setup(m => m.SendCommand<SetBrightnessResponse>(It.IsAny<SetBrightnessCommand>()))
                    .Callback<ICommand>(arg => actual = arg.CommandString);

                scope.Sut.Brightness = brightness;
                Assert.That(actual, Is.EqualTo(command));
            }
        }

        [Test]
        public async Task TestOpen() {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);
                scope.MockSdk.Setup(m => m.SendCommand<OpenResponse>(It.IsAny<OpenCommand>()))
                    .Returns(new OpenResponse { DeviceResponse = "*O99OOO" });
                scope.MockSdk.SetupSequence(m => m.SendCommand<StateResponse>(It.IsAny<StateCommand>()))
                    .Returns(new StateResponse { DeviceResponse = "*S99100" }) //motor running
                    .Returns(new StateResponse { DeviceResponse = "*S99002" }) //motor stopped
                    .Returns(new StateResponse { DeviceResponse = "*S99002" }); //cover is open
                Assert.That(await scope.Sut.Open(It.IsAny<CancellationToken>()), Is.True);
            }
        }

        [Test]
        public async Task TestOpenInvalidResponse() {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);

                scope.MockSdk.Setup(m => m.SendCommand<OpenResponse>(It.IsAny<OpenCommand>()))
                    .Returns(new OpenResponse { DeviceResponse = "*O99XXX" });
                Assert.That(await scope.Sut.Open(It.IsAny<CancellationToken>()), Is.False);
            }
        }

        [Test]
        public async Task TestClose() {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);

                scope.MockSdk.Setup(m => m.SendCommand<CloseResponse>(It.IsAny<CloseCommand>()))
                    .Returns(new CloseResponse { DeviceResponse = "*C99OOO" });
                scope.MockSdk.SetupSequence(m => m.SendCommand<StateResponse>(It.IsAny<StateCommand>()))
                    .Returns(new StateResponse { DeviceResponse = "*S99100" }) //motor running
                    .Returns(new StateResponse { DeviceResponse = "*S99001" }) //motor stopped
                    .Returns(new StateResponse { DeviceResponse = "*S99001" }); //cover is closed
                Assert.That(await scope.Sut.Close(It.IsAny<CancellationToken>()), Is.True);
            }
        }

        [Test]
        public async Task TestCloseInvalidResponse() {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);

                scope.MockSdk.Setup(m => m.SendCommand<CloseResponse>(It.IsAny<CloseCommand>()))
                    .Returns(new CloseResponse { DeviceResponse = "*C99XXX" });
                Assert.That(await scope.Sut.Close(It.IsAny<CancellationToken>()), Is.False);
            }
        }

        [Test]
        [TestCase(">LOOO\r", "*L99OOO")]
        [TestCase(">LOOO\r", null)]
        public async Task TestSetLightOn(string command, string response) {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);

                string actual = null;
                scope.MockSdk.Setup(m => m.SendCommand<LightOnResponse>(It.IsAny<LightOnCommand>()))
                    .Callback<ICommand>(arg => actual = arg.CommandString)
                    .Returns(new LightOnResponse { DeviceResponse = response });
                scope.Sut.LightOn = true;
                Assert.That(actual, Is.EqualTo(command));
            }
        }

        [Test]
        [TestCase(">DOOO\r", "*L99OOO")]
        [TestCase(">DOOO\r", null)]
        public async Task TestSetLightOff(string command, string response) {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);

                string actual = null;
                scope.MockSdk.Setup(m => m.SendCommand<LightOffResponse>(It.IsAny<LightOffCommand>()))
                    .Callback<ICommand>(arg => actual = arg.CommandString)
                    .Returns(new LightOffResponse { DeviceResponse = response });
                scope.Sut.LightOn = false;
                Assert.That(actual, Is.EqualTo(command));
            }
        }

        [Test]
        [TestCase("*S99010", true)]
        [TestCase("*S99000", false)]
        [TestCase("garbage", false)]
        [TestCase(null, false)]
        public async Task TestGetLightOn(string response, bool on) {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);

                scope.MockSdk.Setup(m => m.SendCommand<StateResponse>(It.IsAny<StateCommand>()))
                    .Returns(new StateResponse { DeviceResponse = response });
                Assert.That(scope.Sut.LightOn, Is.EqualTo(on));
            }
        }

        [Test]
        [TestCase("*S99010", false)]
        public void TestGetLightOnDisconnected(string response, bool on) {
            using (var scope = new TestScope()) {
                scope.MockSdk.Setup(m => m.SendCommand<StateResponse>(It.IsAny<StateCommand>()))
                    .Returns(new StateResponse { DeviceResponse = response });
                Assert.That(scope.Sut.LightOn, Is.EqualTo(on));
            }
        }

        [Test]
        public async Task TestConnectedDisconnected() {
            using (var scope = new TestScope()) {
                Assert.That(await scope.Sut.Connect(new CancellationToken()), Is.True);

                scope.Sut.Disconnect();
                Assert.That(scope.Sut.Connected, Is.False);
            }
        }
    }
}