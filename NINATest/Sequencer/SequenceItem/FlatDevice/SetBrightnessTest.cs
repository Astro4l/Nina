﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using FluentAssertions;
using Moq;
using NINA.Core.Enum;
using NINA.Model;
using NINA.Model.MyFlatDevice;
using NINA.Sequencer;
using NINA.Sequencer.Exceptions;
using NINA.Sequencer.SequenceItem.FlatDevice;
using NINA.Utility.Mediator.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINATest.Sequencer.SequenceItem.FlatDevice {

    [TestFixture]
    internal class SetBrightnessTest {
        public Mock<IFlatDeviceMediator> fdMediatorMock;

        [SetUp]
        public void Setup() {
            fdMediatorMock = new Mock<IFlatDeviceMediator>();
        }

        [Test]
        public void Clone_ItemClonedProperly() {
            var sut = new SetBrightness(fdMediatorMock.Object);
            sut.Name = "SomeName";
            sut.Description = "SomeDescription";
            sut.Icon = new System.Windows.Media.GeometryGroup();
            sut.Brightness = 10;

            var item2 = (SetBrightness)sut.Clone();

            item2.Should().NotBeSameAs(sut);
            item2.Name.Should().BeSameAs(sut.Name);
            item2.Description.Should().BeSameAs(sut.Description);
            item2.Icon.Should().BeSameAs(sut.Icon);
            item2.Brightness.Should().Be(sut.Brightness);
        }

        [Test]
        public void Validate_NoIssues() {
            fdMediatorMock.Setup(x => x.GetInfo()).Returns(new FlatDeviceInfo() { Connected = true, SupportsOpenClose = true });

            var sut = new SetBrightness(fdMediatorMock.Object);
            var valid = sut.Validate();

            valid.Should().BeTrue();

            sut.Issues.Should().BeEmpty();
        }

        [Test]
        [TestCase(false, false, 1)]
        [TestCase(false, true, 1)]
        public void Validate_NotConnected_OneIssue(bool isConnected, bool canClose, int count) {
            fdMediatorMock.Setup(x => x.GetInfo()).Returns(new FlatDeviceInfo() { Connected = isConnected, SupportsOpenClose = canClose });

            var sut = new SetBrightness(fdMediatorMock.Object);
            var valid = sut.Validate();

            valid.Should().BeFalse();

            sut.Issues.Should().HaveCount(count);
        }

        [Test]
        public async Task Execute_NoIssues_LogicCalled() {
            fdMediatorMock.Setup(x => x.GetInfo()).Returns(new FlatDeviceInfo() { Connected = true, SupportsOpenClose = true });

            var sut = new SetBrightness(fdMediatorMock.Object);
            sut.Brightness = 10;
            await sut.Execute(default, default);

            fdMediatorMock.Verify(x => x.SetBrightness(It.Is<double>(b => b == sut.Brightness / 100), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public async Task Execute_HasIssues_LogicNotCalled(bool isConnected, bool canClose) {
            fdMediatorMock.Setup(x => x.GetInfo()).Returns(new FlatDeviceInfo() { Connected = isConnected, SupportsOpenClose = canClose });

            var sut = new SetBrightness(fdMediatorMock.Object);
            await sut.Run(default, default);

            fdMediatorMock.Verify(x => x.SetBrightness(It.IsAny<double>(), It.IsAny<CancellationToken>()), Times.Never);
            sut.Status.Should().Be(SequenceEntityStatus.SKIPPED);
        }

        [Test]
        public void GetEstimatedDuration_BasedOnParameters_ReturnsCorrectEstimate() {
            var sut = new SetBrightness(fdMediatorMock.Object);

            var duration = sut.GetEstimatedDuration();

            duration.Should().Be(TimeSpan.Zero);
        }
    }
}