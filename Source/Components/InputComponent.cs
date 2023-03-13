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
public class InputComponent
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
        { Keys.D1, 0x1 }, // 1
        { Keys.D2, 0x2 }, // 2
        { Keys.D3, 0x3 }, // 3
        { Keys.D4, 0xC }, // C
        { Keys.Q, 0x4 }, // 4
        { Keys.W, 0x5 }, // 5
        { Keys.E, 0x6 }, // 6
        { Keys.R, 0xD }, // D
        { Keys.A, 0x7 }, // 7
        { Keys.S, 0x8 }, // 8
        { Keys.D, 0x9 }, // 9
        { Keys.F, 0xE }, // E
        { Keys.Z, 0xA }, // A
        { Keys.X, 0x0 }, // 0
        { Keys.C, 0xB }, // B
        { Keys.V, 0xF } // F
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