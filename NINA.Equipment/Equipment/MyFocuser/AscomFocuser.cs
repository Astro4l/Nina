#region "copyright"

/*
    Copyright � 2016 - 2023 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using ASCOM;
using ASCOM.Common.DeviceInterfaces;
using ASCOM.Com.DriverAccess;
using NINA.Core.Locale;
using NINA.Core.Utility;
using NINA.Equipment.ASCOMFacades;
using NINA.Equipment.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Equipment.Equipment.MyFocuser {

    internal class AscomFocuser : AscomDevice<Focuser, IFocuserFacade, FocuserFacadeProxy>, IFocuser, IDisposable {

        public AscomFocuser(string focuser, string name, IDeviceDispatcher deviceDispatcher) : base(focuser, name, deviceDispatcher, DeviceDispatcherType.Focuser) {
        }

        public IFocuserFacade Device => device;

        public bool IsMoving {
            get {
                return GetProperty(nameof(Focuser.IsMoving), false);
            }
        }

        public int MaxIncrement {
            get {
                return GetProperty(nameof(Focuser.MaxIncrement), -1);
            }
        }

        public int MaxStep {
            get {
                return GetProperty(nameof(Focuser.MaxStep), -1);
            }
        }

        private bool _isAbsolute;

        //Used for relative focusers
        private int internalPosition;

        public int Position {
            get {
                if (_isAbsolute) {
                    return GetProperty(nameof(Focuser.Position), -1); ;
                } else {
                    return internalPosition;
                }
            }
        }

        public double StepSize {
            get {
                return GetProperty(nameof(Focuser.StepSize), double.NaN);
            }
        }

        public bool TempCompAvailable {
            get {
                return GetProperty(nameof(Focuser.TempCompAvailable), false);
            }
        }

        public bool TempComp {
            get {
                if (TempCompAvailable) {
                    return GetProperty(nameof(Focuser.TempComp), false);
                } else {
                    return false;
                }
            }
            set {
                if (Connected && TempCompAvailable) {
                    SetProperty(nameof(Focuser.TempComp), value);
                }
            }
        }

        public double Temperature {
            get {
                return GetProperty(nameof(Focuser.Temperature), double.NaN);
            }
        }

        public Task Move(int position, CancellationToken ct, int waitInMs = 1000) {
            if (_isAbsolute) {
                return MoveInternalAbsolute(position, ct, waitInMs);
            } else {
                return MoveInternalRelative(position, ct, waitInMs);
            }
        }

        private static TimeSpan SameFocuserPositionTimeout = TimeSpan.FromMinutes(1);

        private async Task MoveInternalAbsolute(int position, CancellationToken ct, int waitInMs = 1000) {
            if (Connected) {
                var reEnableTempComp = TempComp;
                if (reEnableTempComp) {
                    TempComp = false;
                }

                var lastPosition = int.MinValue;
                int samePositionCount = 0;
                var lastMovementTime = DateTime.Now;
                while (position != device.Position && !ct.IsCancellationRequested) {
                    device.Move(position);
                    while (IsMoving && !ct.IsCancellationRequested) {
                        await CoreUtil.Wait(TimeSpan.FromMilliseconds(waitInMs), ct);
                    }

                    if (lastPosition == device.Position) {
                        ++samePositionCount;
                        var samePositionTime = DateTime.Now - lastMovementTime;
                        if (samePositionTime >= SameFocuserPositionTimeout) {
                            throw new Exception($"Focuser stuck at position {lastPosition} beyond {SameFocuserPositionTimeout} timeout");
                        }

                        // Make sure we wait in between Move requests when no progress is being made
                        // to avoid spamming the driver and spiking the CPU
                        await CoreUtil.Wait(TimeSpan.FromSeconds(1), ct);
                    } else {
                        lastMovementTime = DateTime.Now;
                    }
                    lastPosition = device.Position;
                }

                if (reEnableTempComp) {
                    TempComp = true;
                }
            }
        }

        private async Task MoveInternalRelative(int position, CancellationToken ct, int waitInMs = 1000) {
            if (Connected) {
                var reEnableTempComp = TempComp;
                if (reEnableTempComp) {
                    TempComp = false;
                }

                var relativeOffsetRemaining = position - this.Position;
                while (relativeOffsetRemaining != 0 && !ct.IsCancellationRequested) {
                    var moveAmount = Math.Min(MaxStep, Math.Abs(relativeOffsetRemaining));
                    if (relativeOffsetRemaining < 0) {
                        moveAmount *= -1;
                    }
                    device.Move(moveAmount);
                    while (IsMoving && !ct.IsCancellationRequested) {
                        await CoreUtil.Wait(TimeSpan.FromMilliseconds(waitInMs), ct);
                    }
                    relativeOffsetRemaining -= moveAmount;
                    internalPosition += moveAmount;
                }

                if (reEnableTempComp) {
                    TempComp = true;
                }
            }
        }

        private bool _canHalt;

        public void Halt() {
            if (Connected && _canHalt) {
                try {
                    device.Halt();
                } catch (MethodNotImplementedException) {
                    _canHalt = false;
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }
        }

        protected override string ConnectionLostMessage => Loc.Instance["LblFocuserConnectionLost"];

        private void Initialize() {
            internalPosition = device.MaxStep / 2;
            _isAbsolute = device.Absolute;
            if (!_isAbsolute) { Logger.Info("The focuser is a relative focuser. Simulating absoute focuser behavior"); }
            _canHalt = true;
        }

        protected override Task PostConnect() {
            Initialize();
            return Task.CompletedTask;
        }

        protected override Focuser GetInstance(string id) {
            return DeviceDispatcher.Invoke(DeviceDispatcherType, () => new Focuser(id));
        }
    }
}