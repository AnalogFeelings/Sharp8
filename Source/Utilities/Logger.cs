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

using Matcha;

namespace Sharp8.Utilities;

/// <summary>
/// Class used for outputting useful logging details to the console.
/// </summary>
public static class Logger
{
    private static readonly MatchaLogger _Logger;

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
}