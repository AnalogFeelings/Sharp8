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

using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Sharp8.Emulator;
namespace Sharp8;

public class MainProgram
{
    public static void Main(string[] Args)
    {
        if (Args.Length == 0 || !File.Exists(Args[0]))
            throw new ArgumentException("You must provide the path to the CHIP-8 program to run.");

        Settings.ProgramPath = Args[0];
        
        NativeWindowSettings windowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(512, 256),
            Title = "Sharp8",
            Flags = ContextFlags.ForwardCompatible
        };
        
        GameWindowSettings gameWindowSettings = GameWindowSettings.Default;
        gameWindowSettings.RenderFrequency = 0;
        gameWindowSettings.UpdateFrequency = 60; // 60Hz

        using (EmulatorWindow emulatorWindow = new EmulatorWindow(gameWindowSettings, windowSettings))
        {
            emulatorWindow.Run();
        }
    }
}