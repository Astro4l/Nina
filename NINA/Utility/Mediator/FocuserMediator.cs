#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Model.MyFocuser;
using NINA.Utility.Mediator.Interfaces;
using NINA.ViewModel.Equipment.Focuser;
using NINA.ViewModel.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Utility.Mediator {

    internal class FocuserMediator : DeviceMediator<IFocuserVM, IFocuserConsumer, FocuserInfo>, IFocuserMediator {

        public void ToggleTempComp(bool tempComp) {
            handler.ToggleTempComp(tempComp);
        }

        public Task<int> MoveFocuser(int position, CancellationToken ct) {
            return handler.MoveFocuser(position, ct);
        }

        public Task<int> MoveFocuserRelative(int position, CancellationToken ct) {
            return handler.MoveFocuserRelative(position, ct);
        }
    }
}