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

using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Model {

    public interface IDevice : INotifyPropertyChanged
    {//接口 INotifyPropertyChanged 用于通知客户端（通常绑定客户端）属性值已更改。
        bool HasSetupDialog { get; }
        string Id { get; }
        string Name { get; }

        string Category { get; }
        bool Connected { get; }
        string Description { get; }
        string DriverInfo { get; }
        string DriverVersion { get; }

        Task<bool> Connect(CancellationToken token);

        void Disconnect();

        void SetupDialog();
    }
}