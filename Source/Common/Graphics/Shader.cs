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

using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Sharp8.Common.Graphics;

/// <summary>
/// Convenience class for handling GLSL raw-text shaders from the Assembly resources.
/// <para/>
/// Partially taken from the LearnOpenTK repository.
/// </summary>
public class Shader : IDisposable
{
    private int _ShaderHandle;
    private bool _Disposed;
    
    private readonly Dictionary<string, int> _UniformLocations = new Dictionary<string, int>();
    
    /// <summary>
    /// Creates an instance of the <see cref="Shader"/> class.
    /// </summary>
    /// <param name="VertexFilename">The filename of the vertex shader.</param>
    /// <param name="FragmentFilename">The filename of the fragment shader.</param>
    /// <remarks>
    /// This can only load from integrated resources.
    /// </remarks>
    /// <exception cref="FileNotFoundException">
    /// Thrown if either <paramref name="VertexFilename"/> or <paramref name="FragmentFilename"/> are not found
    /// inside the assembly resources.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown if there is a compilation or linking error with any shader.
    /// </exception>
    public Shader(string VertexFilename, string FragmentFilename)
    {
        Assembly currentAssembly = Assembly.GetExecutingAssembly();
        
        string vertName = currentAssembly.GetManifestResourceNames().Single(x => x.EndsWith(VertexFilename));
        string fragName = currentAssembly.GetManifestResourceNames().Single(x => x.EndsWith(FragmentFilename));

        string vertSource;
        string fragSource;

        using (Stream? vertStream = currentAssembly.GetManifestResourceStream(vertName))
        {
            if (vertStream == null) 
                throw new FileNotFoundException($"The vertex shader \"{VertexFilename}\" was not found in the program's resources.");

            StreamReader vertReader = new StreamReader(vertStream);

            vertSource = vertReader.ReadToEnd();
        }
        using (Stream? fragStream = currentAssembly.GetManifestResourceStream(fragName))
        {
            if (fragStream == null) 
                throw new FileNotFoundException($"The fragment shader \"{FragmentFilename}\" was not found in the program's resources.");

            StreamReader fragReader = new StreamReader(fragStream);

            fragSource = fragReader.ReadToEnd();
        }

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertSource);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragSource);

        CompileShader(vertexShader);
        CompileShader(fragmentShader);

        _ShaderHandle = GL.CreateProgram();

        GL.AttachShader(_ShaderHandle, vertexShader);
        GL.AttachShader(_ShaderHandle, fragmentShader);

        LinkProgram();

        GL.DetachShader(_ShaderHandle, vertexShader);
        GL.DetachShader(_ShaderHandle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);

        int uniformCount;
        GL.GetProgram(_ShaderHandle, GetProgramParameterName.ActiveUniforms, out uniformCount);

        for (int i = 0; i < uniformCount; i++)
        {
            string key = GL.GetActiveUniform(_ShaderHandle, i, out _, out _);

            int location = GL.GetUniformLocation(_ShaderHandle, key);
            
            _UniformLocations.Add(key, location);
        }
    }
    
    ~Shader()
    {
        GL.DeleteProgram(_ShaderHandle);
    }

    /// <summary>
    /// Compiles a shader.
    /// </summary>
    /// <param name="Shader">The ID of the shader to compile.</param>
    /// <exception cref="Exception">Thrown if there is a compilation error.</exception>
    private void CompileShader(int Shader)
    {
        GL.CompileShader(Shader);
        
        int resultCode;
        GL.GetShader(Shader, ShaderParameter.CompileStatus, out resultCode);
        
        if (resultCode != 1)
        {
            string log = GL.GetShaderInfoLog(Shader);
            
            throw new Exception($"Error occurred whilst compiling fragment shader:\n{log}");
        }
    }

    /// <summary>
    /// Links the shader program.
    /// </summary>
    /// <exception cref="Exception">Thrown if there is a linker error.</exception>
    private void LinkProgram()
    {
        GL.LinkProgram(_ShaderHandle);
        
        int resultCode;
        GL.GetProgram(_ShaderHandle, GetProgramParameterName.LinkStatus, out resultCode);
        if (resultCode != 1)
        {
            string log = GL.GetProgramInfoLog(_ShaderHandle);
            
            throw new Exception($"Error occurred whilst linking shader:\n{log}");
        }
    }
    
    /// <summary>
    /// Activates the shader.
    /// </summary>
    public void Use()
    {
        GL.UseProgram(_ShaderHandle);
    }
    
    /// <summary>
    /// Sets an <see cref="int"/> uniform on the shader.
    /// </summary>
    /// <param name="Name">The name of the uniform.</param>
    /// <param name="Value">The value to set the uniform to.</param>
    /// <exception cref="KeyNotFoundException">Thrown if there is no uniform named <paramref name="Name"/>.</exception>
    public void SetUniform(string Name, int Value)
    {
        if (!_UniformLocations.ContainsKey(Name)) throw new KeyNotFoundException($"The shader has no uniform named \"{Name}\".");
        GL.Uniform1(_UniformLocations[Name], Value);
    }

    /// <summary>
    /// Sets a <see cref="float"/> uniform on the shader.
    /// </summary>
    /// <param name="Name">The name of the uniform.</param>
    /// <param name="Value">The value to set the uniform to.</param>
    /// <exception cref="KeyNotFoundException">Thrown if there is no uniform named <paramref name="Name"/>.</exception>
    public void SetUniform(string Name, float Value)
    {
        if (!_UniformLocations.ContainsKey(Name)) throw new KeyNotFoundException($"The shader has no uniform named \"{Name}\".");
        GL.Uniform1(_UniformLocations[Name], Value);
    }
    
    /// <summary>
    /// Sets a <see cref="Vector4"/> uniform on the shader.
    /// </summary>
    /// <param name="Name">The name of the uniform.</param>
    /// <param name="Value">The value to set the uniform to.</param>
    /// <exception cref="KeyNotFoundException">Thrown if there is no uniform named <paramref name="Name"/>.</exception>
    public void SetUniform(string Name, Vector4 Value)
    {
        if (!_UniformLocations.ContainsKey(Name)) throw new KeyNotFoundException($"The shader has no uniform named \"{Name}\".");
        GL.Uniform4(_UniformLocations[Name], Value);
    }

    /// <summary>
    /// Sets a <see cref="Vector3"/> uniform on the shader.
    /// </summary>
    /// <param name="Name">The name of the uniform.</param>
    /// <param name="Value">The value to set the uniform to.</param>
    /// <exception cref="KeyNotFoundException">Thrown if there is no uniform named <paramref name="Name"/>.</exception>
    public void SetUniform(string Name, Vector3 Value)
    {
        if (!_UniformLocations.ContainsKey(Name)) throw new KeyNotFoundException($"The shader has no uniform named \"{Name}\".");
        GL.Uniform3(_UniformLocations[Name], Value);
    }
    
    /// <summary>
    /// Sets a <see cref="Vector2"/> uniform on the shader.
    /// </summary>
    /// <param name="Name">The name of the uniform.</param>
    /// <param name="Value">The value to set the uniform to.</param>
    /// <exception cref="KeyNotFoundException">Thrown if there is no uniform named <paramref name="Name"/>.</exception>
    public void SetUniform(string Name, Vector2 Value)
    {
        if (!_UniformLocations.ContainsKey(Name)) throw new KeyNotFoundException($"The shader has no uniform named \"{Name}\".");
        GL.Uniform2(_UniformLocations[Name], Value);
    }
    
    /// <summary>
    /// Sets a <see cref="Matrix4"/> uniform on the shader.
    /// </summary>
    /// <param name="Name">The name of the uniform.</param>
    /// <param name="Value">The value to set the uniform to.</param>
    /// <exception cref="KeyNotFoundException">Thrown if there is no uniform named <paramref name="Name"/>.</exception>
    public void SetUniform(string Name, Matrix4 Value)
    {
        if (!_UniformLocations.ContainsKey(Name)) throw new KeyNotFoundException($"The shader has no uniform named \"{Name}\".");
        GL.UniformMatrix4(_UniformLocations[Name], false, ref Value);
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool Disposing)
    {
        if (!_Disposed)
        {
            GL.DeleteProgram(_ShaderHandle);

            _Disposed = true;
        }
    }
}