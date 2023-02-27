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

using Silk.NET.Input;

namespace Sharp8.Components;

public class Keyboard
{
    public byte[] VirtualState = new byte[16];

    private readonly Dictionary<Key, int> _Keymap = new Dictionary<Key, int>()
    {
        { Key.Number1, 0 },
        { Key.Number2, 1 },
        { Key.Number3, 2 },
        { Key.Number4, 3 },
        { Key.Q, 4 },
        { Key.W, 5 },
        { Key.E, 6 },
        { Key.R, 7 },
        { Key.A, 8 },
        { Key.S, 9 },
        { Key.D, 10 },
        { Key.F, 11 },
        { Key.Z, 12 },
        { Key.X, 13 },
        { Key.C, 14 },
        { Key.V, 15 }
    };

    public void KeyboardOnKeyDown(IKeyboard Source, Key Key, int Arg3)
    {
        if (!_Keymap.ContainsKey(Key)) return;

        int keyIndex = _Keymap[Key];

        VirtualState[keyIndex] = 1;
    }
    
    public void KeyboardOnKeyUp(IKeyboard Source, Key Key, int Arg3)
    {
        if (!_Keymap.ContainsKey(Key)) return;

        int keyIndex = _Keymap[Key];

        VirtualState[keyIndex] = 0;
    }
}