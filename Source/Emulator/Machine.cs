﻿#region License Information (GPL v3.0)
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

using Matcha;
using Sharp8.Common;
using Sharp8.Components;

namespace Sharp8.Emulator;

/// <summary>
/// The CPU of the CHIP-8 system.
/// </summary>
public sealed class Machine : IDisposable
{
    public ushort CurrentOpcode;

    public byte[] Registers = new byte[16];

    public ushort Index;
    public ushort ProgramCounter;

    public Stack<ushort> Stack = new Stack<ushort>();

    public byte DelayTimer;

    // Define and initialize all subcomponents.
    public MemoryComponent Memory = new MemoryComponent();
    public GraphicsComponent Graphics = new GraphicsComponent();
    public InputComponent Input = new InputComponent();
    public SoundComponent Sound = new SoundComponent();
    public Random Randomizer = new Random();

    public readonly byte[] MachineFont = new byte[80]
    {
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
    };

    /// <summary>
    /// Initializes the CHIP-8 system.
    /// </summary>
    public void Initialize()
    {
        Logger.Log("Initializing emulator...", LogSeverity.Information);
        
        ProgramCounter = MemoryComponent.PROGRAM_OFFSET;

        // Load font into RAM.
        Memory.LoadArray(MachineFont, 0, 0, MachineFont.Length);

        FileInfo programInfo = new FileInfo(Settings.ProgramPath);
        if (programInfo.Length > MemoryComponent.MAX_PROGRAM_SIZE)
        {
            Logger.Panic($"The program file must be {MemoryComponent.MAX_PROGRAM_SIZE} bytes big at maximum.");
        }

        byte[] programBytes = File.ReadAllBytes(Settings.ProgramPath);
        
        Logger.Log("Loading program into memory...", LogSeverity.Information);
        
        // Load program data into RAM.
        Memory.LoadArray(programBytes, 0, MemoryComponent.PROGRAM_OFFSET, programBytes.Length);
        
        Logger.Log("Successfully loaded program into emulator memory.", LogSeverity.Success);
        Logger.Log("Initializing graphics component...", LogSeverity.Information);

        Graphics.Initialize();
        
        Logger.Log("Initializing sound component...", LogSeverity.Information);
        
        Sound.Initialize();
    }

    /// <summary>
    /// Executes a single CPU cycle.
    /// </summary>
    public void DoCycle()
    {
        CurrentOpcode = (ushort)(Memory[ProgramCounter] << 8 | Memory[ProgramCounter + 1]);

        switch (CurrentOpcode & 0xF000)
        {
            case 0x0000: // 0x0*** - Clear screen and return.
            {
                switch (CurrentOpcode & 0x000F)
                {
                    case 0x0000: // 0x00E0 - Clear screen.
                    {
                        Graphics.Reset();

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x000E: // 0x00EE - Return from subroutine.
                    {
                        ProgramCounter = Stack.Pop();
                        ProgramCounter += 2;

                        break;
                    }
                    default:
                        Logger.Log($"Unknown opcode! 0x{CurrentOpcode:X}", LogSeverity.Warning);
                        break;
                }

                break;
            }
            case 0x1000: // 0x1NNN - Jumps to address NNN.
            {
                ushort jumpAddress = (ushort)(CurrentOpcode & 0x0FFF);
                
                ProgramCounter = jumpAddress;
                
                break;
            }
            case 0x2000: // 0x2NNN - Calls subroutine at NNN.
            {
                ushort jumpAddress = (ushort)(CurrentOpcode & 0x0FFF);
                
                Stack.Push(ProgramCounter);
                ProgramCounter = jumpAddress;
                
                break;
            }
            case 0x3000: // 0x3XNN - Skips the next instruction if VX equals NN.
            {
                int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                byte value = (byte)(CurrentOpcode & 0x00FF);
                
                if (Registers[xRegisterIndex] == value)
                    ProgramCounter += 4;
                else
                    ProgramCounter += 2;

                break;
            }
            case 0x4000: // 0x4XNN - Skips the next instruction if VX does not equal NN.
            {
                int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                byte value = (byte)(CurrentOpcode & 0x00FF);
                
                if (Registers[xRegisterIndex] != value)
                    ProgramCounter += 4;
                else
                    ProgramCounter += 2;

                break;
            }
            case 0x5000: // 0x5XY0 - Skips the next instruction if VX equals VY.
            {
                int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;

                if (Registers[xRegisterIndex] == Registers[yRegisterIndex])
                    ProgramCounter += 4;
                else
                    ProgramCounter += 2;

                break;
            }
            case 0x6000: // 0x6XNN - Sets VX to NN.
            {
                int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                byte value = (byte)(CurrentOpcode & 0x00FF);

                Registers[xRegisterIndex] = value;

                ProgramCounter += 2;

                break;
            }
            case 0x7000: // 0x7XNN - Adds NN to VX (carry flag is not changed).
            {
                int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                byte value = (byte)(CurrentOpcode & 0x00FF);

                Registers[xRegisterIndex] += value;

                ProgramCounter += 2;

                break;
            }
            case 0x8000: // 0x8*** - Group of bitwise and math instructions.
            {
                switch (CurrentOpcode & 0x000F)
                {
                    case 0x0000: // 0x8XY0 - Sets VX to the value of VY.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                        int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;

                        Registers[xRegisterIndex] = Registers[yRegisterIndex];

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0001: // 0x8XY1 - Sets VX to a bitwise OR of VX and VY.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                        int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;
                        
                        Registers[xRegisterIndex] |= Registers[yRegisterIndex];

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0002: // 0x8XY2 - Sets VX to a bitwise AND of VX and VY.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                        int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;
                        
                        Registers[xRegisterIndex] &= Registers[yRegisterIndex];

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0003: // 0x8XY3 - Sets VX to a bitwise XOR of VX and VY.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                        int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;
                        
                        Registers[xRegisterIndex] ^= Registers[yRegisterIndex];

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0004: // 0x8XY4 - Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there is not.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                        int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;

                        // Abuse the fact C# casts byte + byte to int and check if it is larger than 255.
                        int flag = (Registers[xRegisterIndex] + Registers[yRegisterIndex] > 0xFF) ? 1 : 0;

                        Registers[xRegisterIndex] += Registers[yRegisterIndex];
                        Registers[0xF] = (byte)flag;

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0005: // 0x8XY5 - VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there is not.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                        int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;

                        // Check if a borrow occurs by seeing if VX is larger than VY.
                        int flag = (Registers[xRegisterIndex] >= Registers[yRegisterIndex]) ? 1 : 0;

                        Registers[xRegisterIndex] -= Registers[yRegisterIndex];
                        Registers[0xF] = (byte)flag;

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0006: // 0x8XY6 - Stores the least significant bit of VX in VF and then shifts VX to the right by 1.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                        int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;

                        int flag = ((Registers[yRegisterIndex] & 0b00000001) > 0) ? 1 : 0;
                        
                        Registers[xRegisterIndex] = (byte)(Registers[yRegisterIndex] >> 1);
                        Registers[0xF] = (byte)flag;
                        
                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0007: // 0x8XY7 - Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there is not.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                        int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;
                        
                        // Check if a borrow occurs by seeing if VY is larger than VX.
                        int flag = (Registers[yRegisterIndex] >= Registers[xRegisterIndex]) ? 1 : 0;

                        Registers[xRegisterIndex] = (byte)(Registers[yRegisterIndex] - Registers[xRegisterIndex]);
                        Registers[0xF] = (byte)flag;
                        
                        ProgramCounter += 2;

                        break;
                    }
                    case 0x000E: // 0x8XYE - Stores the most significant bit of VX in VF and then shifts VX to the left by 1.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                        int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;

                        int flag = ((Registers[yRegisterIndex] & 0b10000000) > 0) ? 1 : 0;

                        Registers[xRegisterIndex] = (byte)(Registers[yRegisterIndex] << 1);
                        Registers[0xF] = (byte)flag;
                        
                        ProgramCounter += 2;

                        break;
                    }
                    default:
                        Logger.Log($"Unknown opcode! 0x{CurrentOpcode:X}", LogSeverity.Warning);
                        break;
                }

                break;
            }
            case 0x9000: // 0x9XY0 - Skips the next instruction if VX does not equal VY.
            {
                int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;

                if (Registers[xRegisterIndex] != Registers[yRegisterIndex])
                    ProgramCounter += 4;
                else
                    ProgramCounter += 2;

                break;
            }
            case 0xA000: // 0xANNN - Sets I to the address NNN.
            {
                ushort value = (ushort)(CurrentOpcode & 0x0FFF);

                Index = value;

                ProgramCounter += 2;

                break;
            }
            case 0xB000: // 0xBNNN - Jumps to the address NNN plus V0.
            {
                ushort value = (ushort)(CurrentOpcode & 0x0FFF);

                ProgramCounter = (ushort)(Registers[0] + value);

                break;
            }
            case 0xC000: // 0xCXNN - Sets VX to the result of a bitwise and operation on a random number and NN.
            {
                int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                byte value = (byte)(CurrentOpcode & 0x00FF);

                Registers[xRegisterIndex] = (byte)(Randomizer.Next(0, byte.MaxValue + 1) & value);

                ProgramCounter += 2;

                break;
            }
            case 0xD000: // 0xDXYN - Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels.
                         // Each row of 8 pixels is read as bit-coded starting from memory location I; I value does not change after the execution of this instruction.
                         // As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that does not happen.
            {
                int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;
                int yRegisterIndex = (CurrentOpcode & 0x00F0) >> 4;

                ushort spriteX = Registers[xRegisterIndex];
                ushort spriteY = Registers[yRegisterIndex];
                ushort spriteHeight = (ushort)(CurrentOpcode & 0x000F);

                ushort spritePixel;

                Registers[0xF] = 0;

                for (int y = 0; y < spriteHeight; y++)
                {
                    spritePixel = Memory[Index + y];
                    
                    for (int x = 0; x < 8; x++)
                    {
                        if ((spritePixel & (0x80 >> x)) != 0)
                        {
                            int pixelPosition = GraphicsComponent.SCREEN_WIDTH * ((y + spriteY) % GraphicsComponent.SCREEN_HEIGHT) + (x + spriteX) % GraphicsComponent.SCREEN_WIDTH;

                            if (Graphics.Framebuffer[pixelPosition] == 1)
                                Registers[0xF] = 1;
                            Graphics.Framebuffer[pixelPosition] ^= 1;
                        }
                    }
                }

                Graphics.DrawFlag = true;
                ProgramCounter += 2;

                break;
            }
            case 0xE000: // 0xE*** - Code flow based on input.
            {
                switch (CurrentOpcode & 0x00FF)
                {
                    case 0x009E: // 0xEX9E - Skips the next instruction if the key stored in VX is pressed.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        if (Input.VirtualState[Registers[xRegisterIndex]] != 0)
                            ProgramCounter += 4;
                        else
                            ProgramCounter += 2;

                        break;
                    }
                    case 0x00A1: // 0xEXA1 - Skips the next instruction if the key stored in VX is not pressed.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        if (Input.VirtualState[Registers[xRegisterIndex]] == 0)
                            ProgramCounter += 4;
                        else
                            ProgramCounter += 2;

                        break;
                    }
                    default:
                        Logger.Log($"Unknown opcode! 0x{CurrentOpcode:X}", LogSeverity.Warning);
                        break;
                }
                
                break;
            }
            case 0xF000: // 0xF*** - Miscellaneous instructions.
            {
                switch (CurrentOpcode & 0x00FF)
                {
                    case 0x0007: // 0xFX07 - Sets VX to the value of the delay timer.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        Registers[xRegisterIndex] = DelayTimer;

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x000A: // 0xFX0A - A key press is awaited, and then stored in VX.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        bool keyPressed = false;

                        for (byte i = 0; i < 16; i++)
                        {
                            if (Input.VirtualState[i] != 0)
                            {
                                Registers[xRegisterIndex] = i;
                                keyPressed = true;
                            }
                        }

                        if (!keyPressed) return;

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0015: // 0xFX15 - Sets the delay timer to VX.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        DelayTimer = Registers[xRegisterIndex];

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0018: // 0xFX18 - Sets the sound timer to VX.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        Sound.SoundTimer = Registers[xRegisterIndex];

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x001E: // 0xFX1E - Adds VX to I. VF is not affected.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        Index += Registers[xRegisterIndex];

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0029: // 0xFX29 - Sets I to the location of the sprite for the character in VX.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        // Each font sprite is made up by 5 bytes. Multiplying VX by 5 gives us the proper offset.
                        Index = (ushort)(Registers[xRegisterIndex] * 5);

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0033: // 0xFX33 - Stores the binary-coded decimal representation of VX,
                                 // with the hundreds digit in memory at location in I, the tens digit at location I+1,
                                 // and the ones digit at location I+2.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        Memory[Index] = (byte)(Registers[xRegisterIndex] / 100 % 10);
                        Memory[Index + 1] = (byte)(Registers[xRegisterIndex] / 10 % 10);
                        Memory[Index + 2] = (byte)(Registers[xRegisterIndex] % 10);

                        ProgramCounter += 2;

                        break;
                    }
                    case 0x0055: // 0xFX55 - Stores from V0 to VX (including VX) in memory, starting at address I.
                                 // The offset from I is increased by 1 for each value written.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        for (int i = 0; i <= xRegisterIndex; i++)
                        {
                            Memory[Index + i] = Registers[i];
                        }

                        Index += (ushort)(xRegisterIndex + 1);
                        ProgramCounter += 2;
                        
                        break;
                    }
                    case 0x0065: // 0xFX65 - Fills from V0 to VX (including VX) with values from memory, starting at address I.
                                 // The offset from I is increased by 1 for each value read.
                    {
                        int xRegisterIndex = (CurrentOpcode & 0x0F00) >> 8;

                        for (int i = 0; i <= xRegisterIndex; i++)
                        {
                            Registers[i] = Memory[Index + i];
                        }
                        
                        Index += (ushort)(xRegisterIndex + 1);
                        ProgramCounter += 2;
                        
                        break;
                    }
                    default:
                        Logger.Log($"Unknown opcode! 0x{CurrentOpcode:X}", LogSeverity.Warning);
                        break;
                }

                break;
            }
            default:
                Logger.Log($"Unknown opcode! 0x{CurrentOpcode:X}", LogSeverity.Warning);
                break;
        }
    }

    /// <summary>
    /// Decreases the delay timer by 1 if its value is over 0.
    /// </summary>
    public void DoTimerTick()
    {
        if (DelayTimer > 0) 
            DelayTimer--;
    }

    /// <summary>
    /// Plays sound if the sound timer's value is over 0, and decreases it by 1.
    /// </summary>
    public void DoSoundTick()
    {
        if (Sound.SoundTimer > 0)
        {
            if(Settings.EnableSound)
                Sound.PlaySound();
            
            Sound.SoundTimer--;
        }
        else
        {
            Sound.StopSound();
        }
    }

    /// <summary>
    /// Pauses execution and destroys any subcomponent that needs it.
    /// </summary>
    public void Dispose()
    {
        Graphics.Dispose();
        Sound.Dispose();
    }
}