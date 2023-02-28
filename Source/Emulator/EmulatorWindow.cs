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

using System.ComponentModel;
using System.Drawing;
using Matcha;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Sharp8.Utilities;

namespace Sharp8.Emulator;

public class EmulatorWindow : GameWindow
{
    public Machine Machine = new Machine();
    
    public EmulatorWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        // Empty constructor!
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        Machine.Initialize();

        Title = "Sharp8 - " + Path.GetFileName(Settings.ProgramPath);
        
        Logger.Log("The emulator has initialized successfully. Execution will begin.", LogSeverity.Success);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        Machine.DoCycle();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        bool didRender = Machine.MachineGraphics.Render();
        
        if(didRender) SwapBuffers();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        Machine.MachineGraphics.Destroy();
        
        base.OnClosing(e);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        
        GL.Viewport(new Size(e.Size.X, e.Size.Y));
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        if(e.Key == Keys.Escape)
            Close();
        
        Machine.MachineKeyboard.ProcessEvent(e.Key, true);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        
        Machine.MachineKeyboard.ProcessEvent(e.Key, false);
    }
}