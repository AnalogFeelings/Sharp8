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

using Silk.NET.OpenGL;

namespace Sharp8.OpenGL;

/// <summary>
/// Convenience class for handling GLSL raw-text shaders.
/// </summary>
public class Shader : IDisposable
{
    private uint _ShaderHandle;
    private bool _Disposed;
    
    private GL _Gl;
    
    /// <summary>
    /// Creates an instance of the <see cref="Shader"/> class.
    /// </summary>
    /// <param name="GlContext">The OpenGL context.</param>
    /// <param name="VertexPath">The path to the vertex shader.</param>
    /// <param name="FragmentPath">The path to the fragment shader.</param>
    public Shader(GL GlContext, string VertexPath, string FragmentPath)
    {
        _Gl = GlContext;

        string vertSource = File.ReadAllText(VertexPath);
        string fragSource = File.ReadAllText(FragmentPath);

        uint vertexShader = _Gl.CreateShader(ShaderType.VertexShader);
        _Gl.ShaderSource(vertexShader, vertSource);

        uint fragmentShader = _Gl.CreateShader(ShaderType.FragmentShader);
        _Gl.ShaderSource(fragmentShader, fragSource);

        _Gl.CompileShader(vertexShader);
        _Gl.CompileShader(fragmentShader);

        _ShaderHandle = _Gl.CreateProgram();

        _Gl.AttachShader(_ShaderHandle, vertexShader);
        _Gl.AttachShader(_ShaderHandle, fragmentShader);

        _Gl.LinkProgram(_ShaderHandle);

        _Gl.DetachShader(_ShaderHandle, vertexShader);
        _Gl.DetachShader(_ShaderHandle, fragmentShader);
        _Gl.DeleteShader(fragmentShader);
        _Gl.DeleteShader(vertexShader);
    }
    
    ~Shader()
    {
        _Gl.DeleteProgram(_ShaderHandle);
    }
    
    public void SetUniform(string name, int value)
    {
        int location = _Gl.GetUniformLocation(_ShaderHandle, name);
        
        _Gl.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = _Gl.GetUniformLocation(_ShaderHandle, name);
        
        _Gl.Uniform1(location, value);
    }
    
    public void Use()
    {
        _Gl.UseProgram(_ShaderHandle);
    }
    
    protected virtual void Dispose(bool Disposing)
    {
        if (!_Disposed)
        {
            _Gl.DeleteProgram(_ShaderHandle);

            _Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}