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

using FluentAssertions;
using FTD2XX_NET;
using Moq;
using NINA.MGEN.Commands.CompatibilityMode;

using NINA.MGEN.Exceptions;

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NINATest.MGEN.Commands {

    [TestFixture]
    public class QueryDeviceCommandTest : CommandTestRunner {
        private Mock<IFTDI> ftdiMock = new Mock<IFTDI>();

        [Test]
        public void ConstructorTest() {
            var sut = new QueryDeviceCommand();

            sut.CommandCode.Should().Be(0xaa);
            sut.AcknowledgeCode.Should().Be(0x55);
            sut.RequiredBaudRate.Should().Be(9600);
            sut.Timeout.Should().Be(1000);
        }

        [Test]
        public void Successful_AppMode_Scenario_Test() {
            SetupWrite(ftdiMock, new byte[] { 0xaa, 0x01, 0x01 });
            SetupRead(ftdiMock, new byte[] { 0x55, 0x03, 0x01, 0x80, 0x02 });

            var sut = new QueryDeviceCommand();
            var result = sut.Execute(ftdiMock.Object);

            result.Success.Should().BeTrue();
            result.IsBootMode.Should().BeFalse();
        }

        [Test]
        public void Successful_BootMode_Scenario_Test() {
            SetupWrite(ftdiMock, new byte[] { 0xaa, 0x01, 0x01 });
            SetupRead(ftdiMock, new byte[] { 0x55, 0x03, 0x01, 0x80, 0x01 });

            var sut = new QueryDeviceCommand();
            var result = sut.Execute(ftdiMock.Object);

            result.Success.Should().BeTrue();
            result.IsBootMode.Should().BeTrue();
        }

        [Test]
        [TestCase(0x99)]
        [TestCase(0xf0)]
        [TestCase(0xf1)]
        [TestCase(0xf2)]
        [TestCase(0xf3)]
        public void UnexpectedCode_Test(byte errorCode) {
            SetupWrite(ftdiMock, new byte[] { 0xaa, 0x01, 0x01 });
            SetupRead(ftdiMock, new byte[] { errorCode });

            var sut = new QueryDeviceCommand();
            var result = sut.Execute(ftdiMock.Object);

            result.Should().Be(null);
        }
    }
}