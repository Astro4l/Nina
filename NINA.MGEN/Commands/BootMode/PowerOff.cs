#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using FTD2XX_NET;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.MGEN.Commands.BootMode {

    public class PowerOffCommand : BootModeCommand<MGENResult> {
        public override byte CommandCode { get; } = 0xe2;
        public override byte AcknowledgeCode { get; } = 0x00;

        public override MGENResult Execute(IFTDI device) {
            Write(device, CommandCode);
            return new MGENResult(true);
        }
    }
}