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

using System.Reflection;
using System.Runtime.CompilerServices;
using Matcha;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using Sharp8.Emulator;
using Sharp8.Utilities;
using Image = SixLabors.ImageSharp.Image;

namespace Sharp8;

public class MainProgram
{
    public static void Main(string[] Args)
    {
        if (Args.Length == 0)
            Logger.Panic("You must provide the path to the CHIP-8 program to run.");
        if(!File.Exists(Args[0]))
            Logger.Panic("You must provide a valid path to the CHIP-8 program to run. Check your spelling.");

        string programVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString(2);
        Logger.Log($"Sharp8 v{programVersion} by AestheticalZ https://github.com/AestheticalZ", LogSeverity.Information);

        Settings.ProgramPath = Args[0];

        NativeWindowSettings windowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(512, 256),
            Title = "Sharp8",
            Icon = GetWindowIcon(),
            Flags = ContextFlags.ForwardCompatible // Supposedly needed for macOS compatibility.
        };
        
        GameWindowSettings gameWindowSettings = GameWindowSettings.Default;
        gameWindowSettings.RenderFrequency = 0;
        gameWindowSettings.UpdateFrequency = 60; // 60Hz

        using (EmulatorWindow emulatorWindow = new EmulatorWindow(gameWindowSettings, windowSettings))
        {
            emulatorWindow.Run();
        }
    }

    /// <summary>
    /// Read the app icon and return it as a <see cref="WindowIcon"/>.
    /// </summary>
    /// <remarks>
    /// Returns null if the icon fails to load. This shouldn't cause an issue with OpenTK.
    /// </remarks>
    private static WindowIcon? GetWindowIcon()
    {
        try
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string iconName = currentAssembly.GetManifestResourceNames().Single(x => x.EndsWith("window_icon.png"));

            Image<Rgba32> loadedImage;
            using (Stream imageStream = currentAssembly.GetManifestResourceStream(iconName)!)
            {
                loadedImage = (Image<Rgba32>)Image.Load(imageStream);
            }
            
            byte[] imageBytes = new byte[loadedImage.Width * loadedImage.Height * Unsafe.SizeOf<Rgba32>()];
            loadedImage.CopyPixelDataTo(imageBytes);

            return new WindowIcon(new OpenTK.Windowing.Common.Input.Image(loadedImage.Width, loadedImage.Height, imageBytes));
        }
        catch (Exception e)
        {
            Logger.Log($"Error loading window icon. Exception:\n{e}", LogSeverity.Warning);
            
            return null;
        }
    }
}