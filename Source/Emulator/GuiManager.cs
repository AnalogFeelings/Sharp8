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

using ImGuiNET;
using Sharp8.Common;

namespace Sharp8.Emulator;

/// <summary>
/// Handles drawing the ImGui configuration window.
/// </summary>
public class GuiManager
{
    private Machine Machine { get; set; }
    public bool IsVisible { get; set; }

    /// <summary>
    /// Initializes the GUI Manager.
    /// </summary>
    /// <param name="Machine">A reference to the Machine instance.</param>
    public GuiManager(ref Machine Machine)
    {
        this.Machine = Machine;
    }
    
    public void ShowWindow()
    {
        if (!IsVisible)
            return;

        // Return if window is collapsed for optimization.
        if (!ImGui.Begin("Sharp8 Interface"))
        {
            ImGui.End();

            return;
        }
        
        ImGui.Text("This is the user interface for Sharp8.\n" +
                   "You can find settings and debug information here.");
        ImGui.Spacing();
        
        ImGui.Text("Graphics Settings");
        ImGui.Separator();
        
        if (ImGui.ColorEdit3("Foreground", ref Settings.ForegroundColor) ||
            ImGui.ColorEdit3("Background", ref Settings.BackgroundColor))
        {
            Machine.Graphics.DrawFlag = true;
        }
        
        ImGui.Spacing();
        
        ImGui.Text("Sound Settings");
        ImGui.Separator();

        if (ImGui.Checkbox("Enable Sound", ref Settings.EnableSound))
        {
            if(!Settings.EnableSound)
                Machine.Sound.StopSound();
        }

        if (ImGui.SliderInt("Sound Volume", ref Settings.SoundVolume, 0, 100))
        {
            Machine.Sound.SetVolume(Settings.SoundVolume);
        }
        
        ImGui.Spacing();
        
        ImGui.Text("Emulation Settings");
        ImGui.Separator();

        ImGui.SliderInt("Speed", ref Settings.CyclesPerFrame, 1, 64);
        if(ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
            ImGui.SetTooltip("The amount of cycles to execute per frame.");

        ImGui.End();
    }
}