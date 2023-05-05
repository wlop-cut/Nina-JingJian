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

using NINA.Model;
using NINA.Model.MyCamera;
using NINA.Model.MyFilterWheel;
using NUnit.Framework;
using System;
using System.Linq;

namespace NINATest {

    [TestFixture]
    public class CaptureSequenceListTest {

        [Test]
        public void DefaultConstructor_ValueTest() {
            //Arrange
            var l = new CaptureSequenceList();
            //Act

            //Assert
            Assert.AreEqual(string.Empty, l.TargetName, "Targetname");
            Assert.AreEqual(0, l.Count);
            Assert.AreEqual(null, l.ActiveSequence);
            Assert.AreEqual(-1, l.ActiveSequenceIndex);
            Assert.AreEqual(0, l.Delay);
        }

        [Test]
        public void SequenceConstructor_ValueTest() {
            //Arrange
            var seq = new CaptureSequence();
            var l = new CaptureSequenceList(seq);
            //Act

            //Assert
            Assert.AreEqual(string.Empty, l.TargetName, "Targetname");
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(seq, l.ActiveSequence);
            Assert.AreEqual(1, l.ActiveSequenceIndex);
            Assert.AreEqual(0, l.Delay);
        }

        [Test]
        public void GetNextSequence_ModeStandard_Initial() {
            //Arrange
            var seq = new CaptureSequence();
            var seq2 = new CaptureSequence();
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.STANDARD;
            l.Add(seq);
            l.Add(seq2);

            //Act
            var nextSeq = l.Next();

            //Assert
            Assert.AreEqual(string.Empty, l.TargetName, "Targetname");
            Assert.AreSame(seq, nextSeq);
            Assert.AreEqual(2, l.Count);
            Assert.AreSame(seq, l.ActiveSequence);
            Assert.AreEqual(1, l.ActiveSequenceIndex);
            Assert.AreEqual(0, l.Delay);
        }

        [Test]
        public void GetNextSequence_ModeStandardOneDisabled_Initial() {
            //Arrange
            var seq = new CaptureSequence();
            var seq2 = new CaptureSequence() { Enabled = false };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.STANDARD;
            l.Add(seq);
            l.Add(seq2);

            //Act
            var nextSeq = l.Next();

            //Assert
            Assert.AreEqual(string.Empty, l.TargetName, "Targetname");
            Assert.AreSame(seq, nextSeq);
            Assert.AreEqual(1, l.Count);
            Assert.AreSame(seq, l.ActiveSequence);
            Assert.AreEqual(1, l.ActiveSequenceIndex);
            Assert.AreEqual(0, l.Delay);
        }

        [Test]
        public void GetNextSequence_ModeStandardOneDisabledAfter_Initial() {
            //Arrange
            var seq = new CaptureSequence();
            var seq2 = new CaptureSequence();
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.STANDARD;
            l.Add(seq);
            l.Add(seq2);

            l.Items[0].Enabled = false;

            //Act
            var nextSeq = l.Next();

            //Assert
            Assert.AreEqual(string.Empty, l.TargetName, "Targetname");
            Assert.AreSame(seq2, nextSeq);
            Assert.AreEqual(1, l.Count);
            Assert.AreSame(seq2, l.ActiveSequence);
            Assert.AreEqual(2, l.ActiveSequenceIndex);
            Assert.AreEqual(0, l.Delay);
        }

        [Test]
        public void GetNextSequence_ModeStandard_NextSequenceSelected() {
            //Arrange
            var seq = new CaptureSequence() { TotalExposureCount = 2 };
            var seq2 = new CaptureSequence();
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.STANDARD;
            l.Add(seq);
            l.Add(seq2);

            //Act
            var nextSeq = l.Next();
            nextSeq = l.Next();
            nextSeq = l.Next();

            //Assert
            Assert.AreEqual(string.Empty, l.TargetName, "Targetname");
            Assert.AreSame(seq2, nextSeq);
            Assert.AreEqual(2, l.Count);
            Assert.AreSame(seq2, l.ActiveSequence);
            Assert.AreEqual(2, l.ActiveSequenceIndex);
            Assert.AreEqual(0, l.Delay);
        }

        [Test]
        public void GetNextSequence_ModeStandardOneDisabled_NextSequenceSelectedShouldBeEmpty() {
            //Arrange
            var seq = new CaptureSequence() { TotalExposureCount = 2 };
            var seq2 = new CaptureSequence() { Enabled = false };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.STANDARD;
            l.Add(seq);
            l.Add(seq2);

            //Act
            var nextSeq = l.Next();
            nextSeq = l.Next();
            nextSeq = l.Next();

            //Assert
            Assert.AreEqual(string.Empty, l.TargetName, "Targetname");
            Assert.AreSame(null, nextSeq);
            Assert.AreEqual(1, l.Count);
            Assert.AreSame(null, l.ActiveSequence);
            Assert.AreEqual(-1, l.ActiveSequenceIndex);
            Assert.AreEqual(0, l.Delay);
        }

        [Test]
        public void GetNextSequence_ModeStandard_AllFinished() {
            //Arrange
            var seq = new CaptureSequence() { TotalExposureCount = 5 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 5 };
            var seq3 = new CaptureSequence() { TotalExposureCount = 5 };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.STANDARD;

            l.Add(seq);
            l.Add(seq2);
            l.Add(seq3);

            //Act
            CaptureSequence actualSeq;
            while ((actualSeq = l.Next()) != null) {
            }

            //Assert
            Assert.AreEqual(null, l.ActiveSequence);
            Assert.AreEqual(-1, l.ActiveSequenceIndex);
            Assert.AreEqual(0, l.Items.Where(x => x.ProgressExposureCount < x.TotalExposureCount).Count());
        }

        [Test]
        public void GetNextSequence_ModeStandardOneDisabled_AllFinished() {
            //Arrange
            var seq = new CaptureSequence() { TotalExposureCount = 5 };
            var seq2 = new CaptureSequence() { ProgressExposureCount = 0, TotalExposureCount = 5, Enabled = false };
            var seq3 = new CaptureSequence() { TotalExposureCount = 5 };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.STANDARD;

            l.Add(seq);
            l.Add(seq2);
            l.Add(seq3);

            //Act
            CaptureSequence actualSeq;
            while ((actualSeq = l.Next()) != null) {
            }

            //Assert
            Assert.AreEqual(null, l.ActiveSequence);
            Assert.AreEqual(-1, l.ActiveSequenceIndex);
            Assert.AreEqual(1, l.Items.Where(x => x.ProgressExposureCount < x.TotalExposureCount).Count());
            Assert.AreEqual(5, seq.ProgressExposureCount);
            Assert.AreEqual(0, seq2.ProgressExposureCount);
            Assert.AreEqual(5, seq3.ProgressExposureCount);
        }

        [Test]
        public void GetNextSequence_ModeStandard_EmptyListNextNull() {
            //Arrange
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.STANDARD;

            //Act
            var actual = l.Next();

            //Assert
            Assert.AreSame(null, actual);
            Assert.AreEqual(null, l.ActiveSequence);
            Assert.AreEqual(-1, l.ActiveSequenceIndex);
        }

        [Test]
        public void GetNextSequence_ModeStandardOneDisabled_EmptyListNextNull() {
            //Arrange
            var seq = new CaptureSequence() { TotalExposureCount = 5, Enabled = false };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.STANDARD;

            l.Add(seq);

            //Act
            var actual = l.Next();

            //Assert
            Assert.AreSame(null, actual);
            Assert.AreEqual(null, l.ActiveSequence);
            Assert.AreEqual(-1, l.ActiveSequenceIndex);
        }

        [Test]
        public void GetNextSequence_ModeRotate_EmptyListNextNull() {
            //Arrange
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;

            //Act
            var actual = l.Next();

            //Assert
            Assert.AreSame(null, actual);
            Assert.AreEqual(null, l.ActiveSequence);
            Assert.AreEqual(-1, l.ActiveSequenceIndex);
        }

        [Test]
        public void GetNextSequence_ModeRotateOneDisabled_EmptyListNextNull() {
            //Arrange
            var seq = new CaptureSequence() { TotalExposureCount = 5, Enabled = false };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;
            l.Add(seq);

            //Act
            var actual = l.Next();

            //Assert
            Assert.AreSame(null, actual);
            Assert.AreEqual(null, l.ActiveSequence);
            Assert.AreEqual(-1, l.ActiveSequenceIndex);
        }

        [Test]
        public void GetNextSequence_ModeRotate_NextSequenceSelected() {
            //Arrange
            var seq = new CaptureSequence() { TotalExposureCount = 5 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 5 };
            var seq3 = new CaptureSequence() { TotalExposureCount = 5 };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;

            l.Add(seq);
            l.Add(seq2);
            l.Add(seq3);

            //Act
            var actualFirst = l.Next();
            var actualSecond = l.Next();
            var actualThird = l.Next();
            var actualFourth = l.Next();

            //Assert
            Assert.AreSame(seq, actualFirst, "First wrong");
            Assert.AreSame(seq2, actualSecond, "Second wrong");
            Assert.AreSame(seq3, actualThird, "Third wrong");
            Assert.AreSame(seq, actualFourth, "Fourth wrong");
        }

        [Test]
        public void GetNextSequence_ModeRotateOneDisabled_NextSequenceSelected() {
            //Arrange
            var seq = new CaptureSequence() { TotalExposureCount = 5 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 5, Enabled = false };
            var seq3 = new CaptureSequence() { TotalExposureCount = 5, Enabled = false };
            var seq4 = new CaptureSequence() { TotalExposureCount = 5 };
            var seq5 = new CaptureSequence() { TotalExposureCount = 5, Enabled = false };
            var seq6 = new CaptureSequence() { TotalExposureCount = 5 };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;

            l.Add(seq);
            l.Add(seq2);
            l.Add(seq3);
            l.Add(seq4);
            l.Add(seq5);
            l.Add(seq6);

            //Act
            var actualFirst = l.Next();
            var actualSecond = l.Next();
            var actualThird = l.Next();
            var actualFourth = l.Next();

            //Assert
            Assert.AreSame(seq, actualFirst, "First wrong");
            Assert.AreSame(seq4, actualSecond, "Second wrong");
            Assert.AreSame(seq6, actualThird, "Third wrong");
            Assert.AreSame(seq, actualFourth, "Fourth wrong");
        }

        [Test]
        public void GetNextSequence_ModeRotate_FirstEmptySecondSelected() {
            //Arrange
            var seq = new CaptureSequence() { ProgressExposureCount = 0, TotalExposureCount = 0 };
            var seq2 = new CaptureSequence() { ProgressExposureCount = 5, TotalExposureCount = 10 };
            var seq3 = new CaptureSequence() { ProgressExposureCount = 5, TotalExposureCount = 7 };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;

            l.Add(seq);
            l.Add(seq2);
            l.Add(seq3);

            //Act
            var actual = l.Next();

            //Assert
            Assert.AreSame(seq2, actual);
        }

        [Test]
        public void GetNextSequence_ModeRotateOneDisabled_FirstEmptySecondDisabledThirdSelected() {
            //Arrange
            var seq = new CaptureSequence() { ProgressExposureCount = 0, TotalExposureCount = 0 };
            var seq2 = new CaptureSequence() { ProgressExposureCount = 5, TotalExposureCount = 10, Enabled = false };
            var seq3 = new CaptureSequence() { ProgressExposureCount = 5, TotalExposureCount = 7 };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;

            l.Add(seq);
            l.Add(seq2);
            l.Add(seq3);

            //Act
            var actual = l.Next();

            //Assert
            Assert.AreSame(seq3, actual);
        }

        [Test]
        public void GetNextSequence_ModeRotate_AllFinished() {
            //Arrange
            var seq = new CaptureSequence() { TotalExposureCount = 5 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 5 };
            var seq3 = new CaptureSequence() { TotalExposureCount = 5 };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;

            l.Add(seq);
            l.Add(seq2);
            l.Add(seq3);

            //Act
            CaptureSequence actualSeq;
            while ((actualSeq = l.Next()) != null) {
            }

            //Assert
            Assert.AreEqual(null, l.ActiveSequence);
            Assert.AreEqual(-1, l.ActiveSequenceIndex);
            Assert.AreEqual(0, l.Items.Where(x => x.ProgressExposureCount < x.TotalExposureCount || x.ProgressExposureCount > x.TotalExposureCount).Count());
        }

        [Test]
        public void GetNextSequence_ModeRotateOneDisabled_AllFinished() {
            //Arrange
            var seq = new CaptureSequence() { TotalExposureCount = 5 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 5, Enabled = false };
            var seq3 = new CaptureSequence() { TotalExposureCount = 5 };
            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;

            l.Add(seq);
            l.Add(seq2);
            l.Add(seq3);

            //Act
            CaptureSequence actualSeq;
            while ((actualSeq = l.Next()) != null) {
            }

            //Assert
            Assert.AreEqual(null, l.ActiveSequence);
            Assert.AreEqual(-1, l.ActiveSequenceIndex);
            Assert.AreEqual(1, l.Items.Where(x => x.ProgressExposureCount < x.TotalExposureCount || x.ProgressExposureCount > x.TotalExposureCount).Count());
            Assert.AreEqual(0, seq2.ProgressExposureCount);
        }

        [Test]
        public void SetTargetName_ValueTest() {
            //Arrange
            var l = new CaptureSequenceList();
            var target = "Messier 31";
            //Act
            l.TargetName = target;

            //Assert
            Assert.AreEqual(target, l.TargetName);
        }

        [Test]
        public void SetDelay_ValueTest() {
            //Arrange
            var l = new CaptureSequenceList();
            var delay = 5213;
            //Act
            l.Delay = delay;

            //Assert
            Assert.AreEqual(delay, l.Delay);
        }

        [Test]
        public void DeleteSequenceDuringPause_NextItemSelected() {
            var seq = new CaptureSequence() { ProgressExposureCount = 0, TotalExposureCount = 5 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 10 };

            var l = new CaptureSequenceList();
            l.Add(seq);

            l.Next();
            l.Next();
            l.Next();

            l.RemoveAt(l.ActiveSequenceIndex - 1);

            l.Add(seq2);

            Assert.AreEqual(seq2, l.ActiveSequence);
        }

        [Test]
        public void DisableSequenceDuringPause_NextItemSelected() {
            var seq = new CaptureSequence() { ProgressExposureCount = 0, TotalExposureCount = 5 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 10 };

            var l = new CaptureSequenceList();
            l.Add(seq);

            l.Next();
            l.Next();
            l.Next();

            l.Items[l.ActiveSequenceIndex - 1].Enabled = false;

            l.Add(seq2);

            Assert.AreEqual(seq2, l.ActiveSequence);
        }

        [Test]
        public void DeleteSequenceDuringPause_ModeRotate_NextItemSelected() {
            var seq = new CaptureSequence() { ProgressExposureCount = 0, TotalExposureCount = 5 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 10 };

            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;
            l.Add(seq);

            l.Next();
            l.Next();
            l.Next();

            l.RemoveAt(l.ActiveSequenceIndex - 1);

            l.Add(seq2);

            Assert.AreEqual(seq2, l.ActiveSequence);
        }

        [Test]
        public void DisableSequenceDuringPause_ModeRotate_NextItemSelected() {
            var seq = new CaptureSequence() { ProgressExposureCount = 0, TotalExposureCount = 5 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 10 };

            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;
            l.Add(seq);

            l.Next();
            l.Next();
            l.Next();

            l.Items[l.ActiveSequenceIndex - 1].Enabled = false;

            l.Add(seq2);

            Assert.AreEqual(seq2, l.ActiveSequence);
        }

        [Test]
        public void AddFirstSequence_ActiveSequenceSet() {
            var seq = new CaptureSequence() { ProgressExposureCount = 0, TotalExposureCount = 5 };

            var l = new CaptureSequenceList();
            l.Add(seq);

            Assert.AreEqual(seq, l.ActiveSequence);
        }

        [Test]
        public void AddFirstSequenceAfterOneDisabledExists_ActiveSequenceSet() {
            var seq = new CaptureSequence() { ProgressExposureCount = 0, TotalExposureCount = 5, Enabled = false };
            var seq2 = new CaptureSequence() { TotalExposureCount = 10 };
            var l = new CaptureSequenceList();
            l.Add(seq);
            l.Add(seq2);

            Assert.AreEqual(seq2, l.ActiveSequence);
        }

        [Test]
        public void RunSequenceMultipleTimes_NumberOfExposuresCorrect() {
            var seq1 = new CaptureSequence() { TotalExposureCount = 50 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 10 };
            var seq3 = new CaptureSequence() { TotalExposureCount = 30 };

            var l = new CaptureSequenceList();
            l.Add(seq1);

            while (l.Next() != null) { }

            l.Add(seq2);

            while (l.Next() != null) { }

            l.Add(seq3);

            while (l.Next() != null) { }

            Assert.AreEqual(50, seq1.ProgressExposureCount);
            Assert.AreEqual(10, seq2.ProgressExposureCount);
            Assert.AreEqual(30, seq3.ProgressExposureCount);
        }

        [Test]
        public void RunSequenceMultipleTimes_ModeRotate_NumberOfExposuresCorrect() {
            var seq1 = new CaptureSequence() { TotalExposureCount = 50 };
            var seq2 = new CaptureSequence() { TotalExposureCount = 10 };
            var seq3 = new CaptureSequence() { TotalExposureCount = 30 };

            var l = new CaptureSequenceList();
            l.Mode = SequenceMode.ROTATE;
            l.Add(seq1);

            while (l.Next() != null) { }

            l.Add(seq2);

            while (l.Next() != null) { }

            l.Add(seq3);

            while (l.Next() != null) { }

            Assert.AreEqual(50, seq1.ProgressExposureCount);
            Assert.AreEqual(10, seq2.ProgressExposureCount);
            Assert.AreEqual(30, seq3.ProgressExposureCount);
        }

        [Test]
        public void CoordinatesTest_SetCoordinates_RaDecPartialsEqualCoordinates() {
            var l = new CaptureSequenceList();
            var coordinates = new NINA.Utility.Astrometry.Coordinates(10, 10, NINA.Utility.Astrometry.Epoch.J2000, NINA.Utility.Astrometry.Coordinates.RAType.Hours);

            l.Coordinates = coordinates.Transform(NINA.Utility.Astrometry.Epoch.J2000);

            Assert.AreEqual(coordinates.RA, l.RAHours + l.RAMinutes + l.RASeconds);
            Assert.AreEqual(coordinates.Dec, l.DecDegrees + l.DecMinutes + l.DecSeconds);
        }

        [TestCase(5, 10, 15, 5.17083333333333)]
        [TestCase(0, 0, 0, 0)]
        [TestCase(15, 01, 01, 15.01694444444444)]
        [TestCase(0, 0, 1, 0.00027777777)]  //Lower bound
        [TestCase(23, 59, 59, 23.99972222222222)]   //upper bound
        [TestCase(0, 0, 0, 0)]  //Lowest bound
        //[TestCase(24, 0, 0, 0)] //Overflow
        //[TestCase(0, 0, -1, 0)] //Overflow
        public void CoordinatesTest_ManualInput_RACheck(int raHours, int raMinutes, int raSeconds, double expected) {
            var l = new CaptureSequenceList();
            var coordinates = new NINA.Utility.Astrometry.Coordinates(0, 0, NINA.Utility.Astrometry.Epoch.J2000, NINA.Utility.Astrometry.Coordinates.RAType.Hours);

            l.RAHours = raHours;
            l.RAMinutes = raMinutes;
            l.RASeconds = raSeconds;

            Assert.AreEqual(expected, l.Coordinates.RA, 0.000001, "Coordinates failed");
            Assert.AreEqual(raHours, l.RAHours, 0.000001, "Hours failed");
            Assert.AreEqual(raMinutes, l.RAMinutes, 0.000001, "Minutes failed");
            Assert.AreEqual(raSeconds, l.RASeconds, 0.000001, "Seconds failed");
        }

        [TestCase(5, 10, 15, 5.17083333333333)]
        [TestCase(0, 0, 0, 0)]
        [TestCase(15, 01, 01, 15.01694444444444)]
        [TestCase(-15, 01, 01, -15.01694444444444)]
        [TestCase(0, 0, 1, 0.00027777777)] //Low bound
        [TestCase(89, 59, 59, 89.99972222222222)] //high bound
        [TestCase(-90, 0, 0, -90)] //Lowest bound
        [TestCase(90, 0, 0, 90)] //Highest bound
        [TestCase(0, 0, -1, -0.00027777777)] //Low bound
        [TestCase(-89, 59, 59, -89.99972222222222)] //high bound
        //[TestCase(90, 0, 1, 90)] //overflow
        //[TestCase(-90, 0, 1, 90)] //overflow
        public void CoordinatesTest_ManualInput_DecCheck(int decDegrees, int decMinutes, int decSeconds, double expected) {
            var l = new CaptureSequenceList();
            var coordinates = new NINA.Utility.Astrometry.Coordinates(0, 0, NINA.Utility.Astrometry.Epoch.J2000, NINA.Utility.Astrometry.Coordinates.RAType.Hours);

            l.DecDegrees = decDegrees;
            l.DecMinutes = decMinutes;
            l.DecSeconds = decSeconds;

            Assert.AreEqual(expected, l.Coordinates.Dec, 0.000001, "Coordinates failed");
            Assert.AreEqual(decDegrees, l.DecDegrees, 0.000001, "Degrees failed");
            Assert.AreEqual(Math.Abs(decMinutes), l.DecMinutes, 0.000001, "Minutes failed");
            Assert.AreEqual(Math.Abs(decSeconds), l.DecSeconds, 0.000001, "Seconds failed");
        }
    }

    [TestFixture]
    public class CaptureSequenceTest {

        [Test]
        public void DefaultConstructor_ValueTest() {
            //Arrange

            //Act
            var seq = new CaptureSequence();

            //Assert
            Assert.AreEqual(1, seq.Binning.X, "Binning X value not as expected");
            Assert.AreEqual(1, seq.Binning.Y, "Binning X value not as expected");
            Assert.AreEqual(false, seq.Dither, "Dither value not as expected");
            Assert.AreEqual(1, seq.DitherAmount, "DitherAmount value not as expected");
            Assert.AreEqual(1, seq.ExposureTime, "ExposureTime value not as expected");
            Assert.AreEqual(null, seq.FilterType, "FilterType value not as expected");
            Assert.AreEqual(-1, seq.Gain, "Gain value not as expected");
            Assert.AreEqual(CaptureSequence.ImageTypes.LIGHT, seq.ImageType, "ImageType value not as expected");
            Assert.AreEqual(0, seq.ProgressExposureCount, "ProgressExposureCount value not as expected");
            Assert.AreEqual(1, seq.TotalExposureCount, "TotalExposureCount value not as expected");
            Assert.AreEqual(true, seq.Enabled, "Enabled value not as expected");
        }

        [Test]
        public void Constructor_ValueTest() {
            //Arrange
            var exposureTime = 5;
            var imageType = CaptureSequence.ImageTypes.BIAS;
            var filter = new FilterInfo("Red", 1234, 3);
            var binning = new BinningMode(2, 3);
            var exposureCount = 20;

            //Act
            var seq = new CaptureSequence(exposureTime, imageType, filter, binning, exposureCount);

            //Assert
            Assert.AreEqual(binning.X, seq.Binning.X, "Binning X value not as expected");
            Assert.AreEqual(binning.Y, seq.Binning.Y, "Binning X value not as expected");
            Assert.AreEqual(false, seq.Dither, "Dither value not as expected");
            Assert.AreEqual(1, seq.DitherAmount, "DitherAmount value not as expected");
            Assert.AreEqual(exposureTime, seq.ExposureTime, "ExposureTime value not as expected");
            Assert.AreEqual(filter, seq.FilterType, "FilterType value not as expected");
            Assert.AreEqual(-1, seq.Gain, "Gain value not as expected");
            Assert.AreEqual(imageType, seq.ImageType, "ImageType value not as expected");
            Assert.AreEqual(0, seq.ProgressExposureCount, "ProgressExposureCount value not as expected");
            Assert.AreEqual(exposureCount, seq.TotalExposureCount, "TotalExposureCount value not as expected");
            Assert.AreEqual(true, seq.Enabled, "Enabled value not as expected");
        }

        [Test]
        public void ReduceExposureCount_ProgressReflectedCorrectly() {
            //Arrange
            var exposureTime = 5;
            var imageType = CaptureSequence.ImageTypes.BIAS;
            var filter = new FilterInfo("Red", 1234, 3);
            var binning = new BinningMode(2, 3);
            var exposureCount = 20;
            var seq = new CaptureSequence(exposureTime, imageType, filter, binning, exposureCount);

            var exposuresTaken = 5;

            //Act
            for (int i = 0; i < exposuresTaken; i++) {
                seq.ProgressExposureCount++;
            }

            //Assert
            Assert.AreEqual(exposuresTaken, seq.ProgressExposureCount, "ProgressExposureCount value not as expected");
            Assert.AreEqual(exposureCount, seq.TotalExposureCount, "TotalExposureCount value not as expected");
        }
    }
}