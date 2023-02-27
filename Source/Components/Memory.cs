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

namespace Sharp8.Components;

/// <summary>
/// Memory module.
/// <para/>
/// Contains the system memory and utility functions.
/// </summary>
public class Memory
{
    public const int MEMORY_SIZE = 4096;
    public const int PROGRAM_OFFSET = 512;
    
    private readonly byte[] _InternalMemory = new byte[MEMORY_SIZE];

    /// <summary>
    /// Copies an <see cref="Array"/> into the internal memory.
    /// </summary>
    /// <param name="Source">The source <see cref="Array"/>.</param>
    /// <param name="SourceIndex">The index at source in which the copying begins.</param>
    /// <param name="DestinationIndex">The index at the internal memory where the storing begins.</param>
    /// <param name="ElementCount">The number of elements to copy.</param>
    public void LoadArray(Array Source, int SourceIndex, int DestinationIndex, int ElementCount)
    {
        Array.Copy(Source, SourceIndex, _InternalMemory, DestinationIndex, ElementCount);
    }

    /// <summary>
    /// Dumps all the memory contents into a file.
    /// </summary>
    public void DumpMemory()
    {
        File.WriteAllBytes("MemoryDump-" + DateTime.Now.ToString().Replace(':', '.') + ".bin", _InternalMemory);
    }

    /// <summary>
    /// Resets the RAM by setting it to 0.
    /// </summary>
    public void Reset()
    {
        Array.Clear(_InternalMemory, 0, _InternalMemory.Length);
    }

    public byte this[int i]
    {
        get => _InternalMemory[i];
        set => _InternalMemory[i] = value;
    }
}