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

using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = Sharp8.OpenGL.Shader;

namespace Sharp8.Components;

public class Graphics
{
    public const int SCREEN_WIDTH = 64;
    public const int SCREEN_HEIGHT = 32;
    
    public bool DrawFlag;
    
    public uint[] Framebuffer = new uint[SCREEN_WIDTH * SCREEN_HEIGHT];
    
    #region OpenGL
    
    private GL _Gl = default!;
    
    private byte[,,] _TextureData = new byte[SCREEN_HEIGHT, SCREEN_WIDTH, 3];

    private uint _TextureHandle;
    
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
    public unsafe void Initialize(GL GlContext)
    {
        _Gl = GlContext;
        
        _Gl.Viewport(MainProgram.AppWindow.Size);

        // Initialize buffers.
        _Gl.GenVertexArrays(1, out _QuadVao);
        _Gl.GenBuffers(1, out _QuadVbo);
        _Gl.GenBuffers(1, out _QuadEbo);
        
        _Gl.BindVertexArray(_QuadVao);
        
        // Initialize VBO.
        _Gl.BindBuffer(GLEnum.ArrayBuffer, _QuadVbo);
        fixed (void* vertices = &_QuadVertices[0])
        {
            _Gl.BufferData(GLEnum.ArrayBuffer, (nuint)(_QuadVertices.Length * sizeof(float)), vertices, GLEnum.StaticDraw);
        }
        
        // Initialize EBO.
        _Gl.BindBuffer(GLEnum.ElementArrayBuffer, _QuadEbo);
        fixed (void* indices = &_QuadIndices[0])
        {
            _Gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(_QuadIndices.Length * sizeof(uint)), indices, GLEnum.StaticDraw);
        }
        
        // Tell OpenGL about the vertex coords.
        _Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), null);
        _Gl.EnableVertexAttribArray(0);
        
        // Tell OpenGL about the UV coords.
        _Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
        _Gl.EnableVertexAttribArray(1);

        // Create quad shader.
        _QuadShader = new Shader(_Gl, "screen.vert", "screen.frag");
        
        // Initialize screen texture.
        _TextureHandle = _Gl.GenTexture();
        
        _Gl.BindTexture(GLEnum.Texture2D, _TextureHandle);
        
        _Gl.PixelStore(GLEnum.UnpackAlignment, 1);
        _Gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgb, SCREEN_WIDTH, SCREEN_HEIGHT, 0, PixelFormat.Rgb, PixelType.UnsignedByte, null);
        
        _Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
        _Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
        _Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
        _Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);
        
        _Gl.Enable(GLEnum.Texture2D);
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
    public unsafe void Render()
    {
        if (!DrawFlag) return;

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
        
        _Gl.ActiveTexture(GLEnum.Texture0);
        _Gl.BindTexture(GLEnum.Texture2D, _TextureHandle);

        fixed (void* textureData = &_TextureData[0, 0, 0])
        {
            _Gl.TexSubImage2D(GLEnum.Texture2D, 0, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, GLEnum.Rgb, GLEnum.UnsignedByte, textureData);
        }
        
        _Gl.ClearColor(Color.Teal);
        _Gl.Clear(ClearBufferMask.ColorBufferBit);
        
        _Gl.BindVertexArray(_QuadVao);
        _QuadShader!.Use();
        
        _Gl.DrawElements(PrimitiveType.Triangles, (uint)_QuadIndices.Length, DrawElementsType.UnsignedInt, null);
        
        MainProgram.AppWindow.SwapBuffers();
        
        DrawFlag = false;
    }
    
    public void WindowOnResize(Vector2D<int> NewSize)
    {
        _Gl.Viewport(NewSize);

        DrawFlag = true;
    }

    /// <summary>
    /// Disposes any resource that needs it.
    /// </summary>
    public void Destroy()
    {
        _Gl.DeleteVertexArrays(1, _QuadVao);
        _Gl.DeleteBuffers(1, _QuadVbo);
        _Gl.DeleteBuffers(1, _QuadEbo);

        _QuadShader?.Dispose();
        
        _Gl.DeleteTexture(_TextureHandle);
    }
}