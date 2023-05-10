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

using System.ComponentModel;
using ImGuiNET;
using Matcha;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Sharp8.Common;
using Sharp8.Common.Graphics;
using Size = System.Drawing.Size;

namespace Sharp8.Emulator;

public class EmulatorWindow : GameWindow
{
    public Machine Machine = new Machine();
    public ImGuiHelper ImGuiHelper = default!; // Prevent nullable initialization warning.
    public GuiManager GuiManager = default!;

    public bool IsClosing; // Workaround for annoying panic on closing.
    
    public EmulatorWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        // Empty constructor!
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
#if DEBUG
        // Initialize OpenGL logging only in Debug configuration.
        Logger.InitializeGlLogging();
#endif
        
        ImGuiHelper = new ImGuiHelper(ClientSize.X, ClientSize.Y);
        GuiManager = new GuiManager(ref Machine);
        
        Machine.Initialize();

        Title = "Sharp8 - " + Path.GetFileName(Settings.ProgramPath);
        
        Logger.Log("The emulator has initialized successfully. Execution will begin.", LogSeverity.Success);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        if (IsClosing)
            return;

        // TODO: Make this configurable in the ImGui settings UI.
        for (int i = 0; i < Settings.CyclesPerFrame; i++)
        {
            Machine.DoCycle();
        }
        
        Machine.DoTimerTick();
        Machine.DoSoundTick();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        if (IsClosing)
            return;
        
        // Counter-intuitively, this should be here instead of OnUpdateFrame.
        // If placed in OnUpdateFrame, input issues arise.
        ImGuiHelper.Update(this, (float)args.Time);
        Machine.Graphics.Render();
        
        GuiManager.ShowWindow();
        ImGuiHelper.Render();
        
        SwapBuffers();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        IsClosing = true;
        
        ImGuiHelper.Dispose();
        Machine.Dispose();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        
        GL.Viewport(new Size(e.Size.X, e.Size.Y));
        ImGuiHelper.Resize(e.Size.X, e.Size.Y);
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (ImGuiHelper.WantsKeyboardInput()) return;

        if (e.Key == Keys.Tab)
        {
            GuiManager.IsVisible = !GuiManager.IsVisible;
            return;
        }

        Machine.Input.ProcessEvent(e.Key, true);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        
        if (ImGuiHelper.WantsKeyboardInput()) return;
        
        Machine.Input.ProcessEvent(e.Key, false);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        
        if (!ImGuiHelper.WantsKeyboardInput()) return;
        
        ImGuiHelper.PressChar((char)e.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        
        ImGuiHelper.MouseScroll(e.Offset);
    }
}