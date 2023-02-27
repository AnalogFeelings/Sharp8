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

using OpenTK.Graphics.OpenGL4;

namespace Sharp8.OpenGL;

/// <summary>
/// Convenience class for handling GLSL raw-text shaders.
/// </summary>
public class Shader : IDisposable
{
    private int _ShaderHandle;
    private bool _Disposed;
    
    /// <summary>
    /// Creates an instance of the <see cref="Shader"/> class.
    /// </summary>
    /// <param name="VertexPath">The path to the vertex shader.</param>
    /// <param name="FragmentPath">The path to the fragment shader.</param>
    public Shader(string VertexPath, string FragmentPath)
    {
        string vertSource = File.ReadAllText(VertexPath);
        string fragSource = File.ReadAllText(FragmentPath);

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertSource);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragSource);

        GL.CompileShader(vertexShader);
        GL.CompileShader(fragmentShader);

        _ShaderHandle = GL.CreateProgram();

        GL.AttachShader(_ShaderHandle, vertexShader);
        GL.AttachShader(_ShaderHandle, fragmentShader);

        GL.LinkProgram(_ShaderHandle);

        GL.DetachShader(_ShaderHandle, vertexShader);
        GL.DetachShader(_ShaderHandle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);
    }
    
    ~Shader()
    {
        GL.DeleteProgram(_ShaderHandle);
    }
    
    public void SetUniform(string name, int value)
    {
        int location = GL.GetUniformLocation(_ShaderHandle, name);
        
        GL.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = GL.GetUniformLocation(_ShaderHandle, name);
        
        GL.Uniform1(location, value);
    }
    
    public void Use()
    {
        GL.UseProgram(_ShaderHandle);
    }
    
    protected virtual void Dispose(bool Disposing)
    {
        if (!_Disposed)
        {
            GL.DeleteProgram(_ShaderHandle);

            _Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}