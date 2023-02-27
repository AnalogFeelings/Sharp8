#region License Information (GPL v3.0)
// Sharp8 - A very simple CHIP-8 emulator based on OpenGL.
// Copyright (C) 2023 AestheticalZ
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Sharp8.Components;

public class Keyboard
{
    public byte[] VirtualState = new byte[16];

    private readonly Dictionary<Keys, int> _Keymap = new Dictionary<Keys, int>()
    {
        { Keys.D1, 0 },
        { Keys.D2, 1 },
        { Keys.D3, 2 },
        { Keys.D4, 3 },
        { Keys.Q, 4 },
        { Keys.W, 5 },
        { Keys.E, 6 },
        { Keys.R, 7 },
        { Keys.A, 8 },
        { Keys.S, 9 },
        { Keys.D, 10 },
        { Keys.F, 11 },
        { Keys.Z, 12 },
        { Keys.X, 13 },
        { Keys.C, 14 },
        { Keys.V, 15 }
    };

    public void ProcessEvent(Keys Source, bool IsDown)
    {
        if (!_Keymap.ContainsKey(Source)) return;

        int keyIndex = _Keymap[Source];

        VirtualState[keyIndex] = (byte)(IsDown ? 1 : 0);
    }
}