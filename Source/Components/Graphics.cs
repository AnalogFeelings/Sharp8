﻿#region License Information (GPL v3.0)
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

using OpenTK.Graphics.OpenGL4;
using Shader = Sharp8.OpenGL.Shader;

namespace Sharp8.Components;

/// <summary>
/// Graphics module for the system.
/// <para/>
/// Handles displaying to the screen.
/// </summary>
public class Graphics
{
    public const int SCREEN_WIDTH = 64;
    public const int SCREEN_HEIGHT = 32;
    
    public bool DrawFlag;
    
    public uint[] Framebuffer = new uint[SCREEN_WIDTH * SCREEN_HEIGHT];
    
    #region OpenGL
    
    private byte[,,] _TextureData = new byte[SCREEN_HEIGHT, SCREEN_WIDTH, 3];

    private int _TextureHandle;
    
    private uint _QuadVbo;
    private uint _QuadVao;
    private uint _QuadEbo;

    private Shader? _QuadShader;

    private readonly float[] _QuadVertices = new float[20]
    {
        //X **** Y **** Z ** U ** V
        +1.0f, +1.0f, -0.0f, 1f, 0f,
        +1.0f, -1.0f, -0.0f, 1f, 1f,
        -1.0f, -1.0f, -0.0f, 0f, 1f,
        -1.0f, +1.0f, -0.0f, 0f, 0f
    };
    private readonly uint[] _QuadIndices = new uint[6]
    {
        0, 1, 3,
        1, 2, 3
    };
    
    #endregion

    /// <summary>
    /// Initializes the graphics module.
    /// </summary>
    public void Initialize()
    {
        // Initialize buffers.
        GL.GenVertexArrays(1, out _QuadVao);
        GL.GenBuffers(1, out _QuadVbo);
        GL.GenBuffers(1, out _QuadEbo);
        
        GL.BindVertexArray(_QuadVao);
        
        // Initialize VBO.
        GL.BindBuffer(BufferTarget.ArrayBuffer, _QuadVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _QuadVertices.Length * sizeof(float), _QuadVertices, BufferUsageHint.StaticDraw);
        
        // Initialize EBO.
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _QuadEbo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _QuadIndices.Length * sizeof(uint), _QuadIndices, BufferUsageHint.StaticDraw);
        
        // Tell OpenGL about the vertex coords.
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        // Tell OpenGL about the UV coords.
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Create quad shader.
        _QuadShader = new Shader("screen.vert", "screen.frag");
        
        // Initialize screen texture.
        _TextureHandle = GL.GenTexture();
        
        GL.BindTexture(TextureTarget.Texture2D, _TextureHandle);
        
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, SCREEN_WIDTH, SCREEN_HEIGHT, 0, PixelFormat.Rgb, PixelType.UnsignedByte, 0);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        
        GL.Enable(EnableCap.Texture2D);
    }
    
    /// <summary>
    /// Resets the graphics by clearing the framebuffer.
    /// </summary>
    public void Reset()
    {
        Array.Clear(Framebuffer, 0, Framebuffer.Length);
    }

    /// <summary>
    /// Renders to the screen if <see cref="DrawFlag"/> is true.
    /// </summary>
    public bool Render()
    {
        if (!DrawFlag) return false;

        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {
                int pixelIndex = y * SCREEN_WIDTH + x;

                if (Framebuffer[pixelIndex] == 0)
                    _TextureData[y, x, 0] = _TextureData[y, x, 1] = _TextureData[y, x, 2] = 0;
                else
                    _TextureData[y, x, 0] = _TextureData[y, x, 1] = _TextureData[y, x, 2] = 255;
            }
        }
        
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _TextureHandle);
        
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, PixelFormat.Rgb, PixelType.UnsignedByte, _TextureData);
        
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        GL.BindVertexArray(_QuadVao);
        _QuadShader!.Use();
        
        GL.DrawElements(PrimitiveType.Triangles, _QuadIndices.Length, DrawElementsType.UnsignedInt, 0);

        DrawFlag = false;
        return true;
    }

    /// <summary>
    /// Disposes any resource that needs it.
    /// </summary>
    public void Destroy()
    {
        GL.DeleteVertexArrays(1, ref _QuadVao);
        GL.DeleteBuffers(1, ref _QuadVbo);
        GL.DeleteBuffers(1, ref _QuadEbo);

        _QuadShader?.Dispose();
        
        GL.DeleteTexture(_TextureHandle);
    }
}