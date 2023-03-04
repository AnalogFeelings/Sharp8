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

using System.Diagnostics;
using System.Runtime.InteropServices;
using Matcha;
using OpenTK.Graphics.OpenGL4;

namespace Sharp8.Common;

/// <summary>
/// Class used for outputting useful logging details to the console.
/// </summary>
public static class Logger
{
    private static readonly MatchaLogger _Logger;

    private static DebugProc _DebugProcCallback = GlDebugCallback;
    private static GCHandle _DebugProcCallbackHandle;

    static Logger()
    {
        MatchaLoggerSettings loggerSettings = new MatchaLoggerSettings()
        {
            LogToFile = false,
#if !DEBUG
            AllowedSeverities = LogSeverity.Information | LogSeverity.Warning | LogSeverity.Error | LogSeverity.Fatal | LogSeverity.Success
#endif
        };

        _Logger = new MatchaLogger(loggerSettings);
    }

    /// <summary>
    /// Initializes OpenGL logging callbacks.
    /// </summary>
    public static void InitializeGlLogging()
    {
        // Set up OpenGL information logging.
        _DebugProcCallbackHandle = GCHandle.Alloc(_DebugProcCallback);
        
        GL.DebugMessageCallback(_DebugProcCallback, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
    }

    /// <summary>
    /// Logs a message to the console with the specified severity.
    /// </summary>
    /// <param name="Message">The message to print out.</param>
    /// <param name="Severity">The severity of the message.</param>
    /// <remarks>
    /// The <see cref="LogSeverity.Debug"/> severity is filtered out if the program isn't running in Debug configuration.
    /// </remarks>
    public static void Log(string Message, LogSeverity Severity)
    {
        _Logger.Log(Message, Severity);
    }
    
    /// <summary>
    /// Logs a fatal error to the console and exits with an error code.
    /// </summary>
    /// <param name="Message">The message to print out.</param>
    /// <remarks>
    /// This method will also break into the debugger, if it is connected.
    /// </remarks>
    public static void Panic(string Message)
    {
        _Logger.Log(Message, LogSeverity.Fatal);
        
        Debugger.Break();
        
        Environment.Exit(-1);
    }
    
    private static void GlDebugCallback(DebugSource Source, DebugType Type, int Id,
        DebugSeverity Severity, int Length, IntPtr Message, IntPtr UserParam)
    {
        string messageString = Marshal.PtrToStringAnsi(Message, Length);
        
        LogSeverity matchaSeverity;
        switch (Severity)
        {
            case DebugSeverity.DebugSeverityHigh:
                matchaSeverity = LogSeverity.Fatal;
                break;
            case DebugSeverity.DebugSeverityMedium:
            case DebugSeverity.DebugSeverityLow:
                matchaSeverity = LogSeverity.Warning;
                break;
            case DebugSeverity.DebugSeverityNotification:
                matchaSeverity = LogSeverity.Information;
                break;
            default:
                matchaSeverity = LogSeverity.Information;
                break;
        }
        
        if(matchaSeverity == LogSeverity.Fatal)
            Panic(messageString);
        else
            Log(messageString, matchaSeverity);
    }
}