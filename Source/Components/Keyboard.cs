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

/// <summary>
/// Keyboard module for the system.
/// <para/>
/// Translates normal PC keycodes into CHIP-8 keycodes.
/// </summary>
public class Keyboard
{
    /// <summary>
    /// Holds the current state of the keyboard in the CHIP-8 machine.
    /// </summary>
    public byte[] VirtualState = new byte[16];

    /// <remarks>
    /// For a better illustration of the keyboard layout, go to http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#keyboard
    /// </remarks>
    private readonly Dictionary<Keys, int> _Keymap = new Dictionary<Keys, int>()
    {
        { Keys.D1, 0 }, // 1
        { Keys.D2, 1 }, // 2
        { Keys.D3, 2 }, // 3
        { Keys.D4, 3 }, // C
        { Keys.Q, 4 }, // 4
        { Keys.W, 5 }, // 5
        { Keys.E, 6 }, // 6
        { Keys.R, 7 }, // D
        { Keys.A, 8 }, // 7
        { Keys.S, 9 }, // 8
        { Keys.D, 10 }, // 9
        { Keys.F, 11 }, // E
        { Keys.Z, 12 }, // A
        { Keys.X, 13 }, // 0
        { Keys.C, 14 }, // B
        { Keys.V, 15 } // F
    };

    /// <summary>
    /// Processes an input event and sets the virtual state of the keyboard accordingly.
    /// </summary>
    /// <param name="Source">The key that was updated.</param>
    /// <param name="IsDown">If the key is pressed down.</param>
    public void ProcessEvent(Keys Source, bool IsDown)
    {
        if (!_Keymap.ContainsKey(Source)) return;

        int keyIndex = _Keymap[Source];

        VirtualState[keyIndex] = (byte)(IsDown ? 1 : 0);
    }
}