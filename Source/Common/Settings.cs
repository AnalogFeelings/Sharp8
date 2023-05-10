#region License Information (GPL v3.0)
// Sharp8 - A very simple CHIP-8 emulator based on OpenGL.
// Copyright (C) 2023 Analog Feelings
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

using System.Numerics;

namespace Sharp8.Common;

public static class Settings
{
    public static string ProgramPath = string.Empty;

    public static Vector3 BackgroundColor = new Vector3(0, 0, 0);
    public static Vector3 ForegroundColor = new Vector3(1, 1, 1);

    public static int CyclesPerFrame = 8;
    public static bool EnableSound = true;

    public static int SoundVolume = 100;
}