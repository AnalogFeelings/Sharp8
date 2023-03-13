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

using System.Runtime.InteropServices;
using Matcha;
using OpenTK.Audio.OpenAL;
using Sharp8.Common;

namespace Sharp8.Components;

/// <summary>
/// The sound module of the CHIP-8 system.
/// </summary>
public class Sound
{
    private const int _SAMPLE_RATE = 44100;
    private const int _WAVE_FREQUENCY = 440;
    private const int _WAVE_AMPLITUDE = 32760;
    private const int _BUFFER_DURATION = 1;
    private const int _BUFFER_SIZE = _BUFFER_DURATION * _SAMPLE_RATE;
    
    public byte SoundTimer;

    private ALDevice _SoundDevice;
    private ALContext _SoundContext;

    private int _SoundSource;
    private int _SoundBufferId;

    private short[] _SoundBuffer = new short[_BUFFER_SIZE];

    private float _WavePhase;

    /// <summary>
    /// Initializes the Sound module.
    /// </summary>
    public void Initialize()
    {
        Logger.Log("Initializing OpenAL device and context objects...", LogSeverity.Debug);
        
        _SoundDevice = ALC.OpenDevice(null);
        _SoundContext = ALC.CreateContext(_SoundDevice, Array.Empty<int>());

        ALC.MakeContextCurrent(_SoundContext);
        
        Logger.Log("Generating OpenAL buffers and sources...", LogSeverity.Debug);

        AL.GenSource(out _SoundSource);
        AL.GenBuffer(out _SoundBufferId);

        Logger.Log("Sound initialization complete.", LogSeverity.Debug);
    }

    /// <summary>
    /// Plays a square wave if <see cref="SoundTimer"/> is > 0.
    /// </summary>
    public void PlaySound()
    {
        if (SoundTimer == 0) return;
        
        GenerateSoundFrame();
        
        GCHandle bufferHandle = GCHandle.Alloc(_SoundBuffer, GCHandleType.Pinned);
        IntPtr bufferPointer = bufferHandle.AddrOfPinnedObject();
        
        AL.BufferData(_SoundBufferId, ALFormat.Mono16, bufferPointer, _SoundBuffer.Length * sizeof(short), _SAMPLE_RATE);
        
        AL.Source(_SoundSource, ALSourcei.Buffer, _SoundBufferId);
        AL.SourcePlay(_SoundSource);
        
        bufferHandle.Free();
    }

    /// <summary>
    /// Disposes all the OpenAL objects.
    /// </summary>
    public void Destroy()
    {
        AL.DeleteSource(_SoundSource);
        AL.DeleteBuffer(_SoundBufferId);

        if (_SoundContext != ALContext.Null)
        {
            ALC.MakeContextCurrent(ALContext.Null);
            ALC.DestroyContext(_SoundContext);
            
            _SoundContext = ALContext.Null;
        }

        if (_SoundDevice != ALDevice.Null)
        {
            ALC.CloseDevice(_SoundDevice);
            
            _SoundDevice = ALDevice.Null;
        }
    }

    /// <summary>
    /// Generates the square wave data for the current frame.
    /// </summary>
    private void GenerateSoundFrame()
    {
        // Avoid loss of fraction warning.
        float period = (float)_SAMPLE_RATE / _WAVE_FREQUENCY;

        for (int i = 0; i < _SoundBuffer.Length; i++)
        {
            if (_WavePhase < period / 2.0f)
            {
                _SoundBuffer[i] = _WAVE_AMPLITUDE;
            }
            else
            {
                _SoundBuffer[i] = -_WAVE_AMPLITUDE;
            }
            
            _WavePhase = (_WavePhase + 1.0f) % period;
        }
    }
}