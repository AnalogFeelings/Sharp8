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

using Sharp8.Emulator;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Sharp8;

public class MainProgram
{
    public Machine Machine = new Machine();

    public string ProgramPath = string.Empty;

    public static GL? Gl;
    public static IWindow AppWindow = default!;
    
    public void EmulatorEntry(string[] Args)
    {
        if (Args.Length == 0 || !File.Exists(Args[0]))
            throw new ArgumentException("You must provide the path to the CHIP-8 program to run.");
        
        ProgramPath = Args[0];

        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.Size = new Vector2D<int>(512, 256);
        windowOptions.Title = "Sharp8";
        windowOptions.FramesPerSecond = 60; // 60Hz
        
        AppWindow = Window.Create(windowOptions);
        
        AppWindow.Load += WindowOnLoad;
        AppWindow.Render += WindowOnRender;
        AppWindow.Closing += WindowOnClosing;

        AppWindow.Run();
    }

    private void WindowOnLoad()
    {
        IInputContext inputContext = AppWindow.CreateInput();
        foreach (IKeyboard keyboard in inputContext.Keyboards)
        {
            keyboard.KeyDown += Machine.MachineKeyboard.KeyboardOnKeyDown;
            keyboard.KeyUp += Machine.MachineKeyboard.KeyboardOnKeyUp;
        }

        Gl = GL.GetApi(AppWindow);

        Machine.Initialize(Gl, ProgramPath);

        AppWindow.Resize += Machine.MachineGraphics.WindowOnResize;
    }
    
    private void WindowOnRender(double Obj)
    {
        Machine.DoCycle();
        
        Machine.MachineGraphics.Render();
    }

    private void WindowOnClosing()
    {
        Machine.MachineGraphics.Destroy();
        
        Gl?.Dispose();
    }

    public static void Main(string[] Args)
    {
        MainProgram mainProgram = new MainProgram();
        
        mainProgram.EmulatorEntry(Args);
    }
}