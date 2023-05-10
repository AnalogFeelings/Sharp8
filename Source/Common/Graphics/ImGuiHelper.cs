#region License Information (GPL v3.0)
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

using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Sharp8.Common.Graphics;

/// <summary>
/// Handles the context stuff for ImGui.
/// <para/>
/// Based on the code in: https://github.com/NogginBops/ImGui.NET_OpenTK_Sample
/// </summary>
public class ImGuiHelper : IDisposable
{
    private bool _FrameBegunRendering;

    private int _GuiVao;
    private int _GuiVbo;
    private int _GuiVboSize = 10000;
    private int _GuiEbo;
    private int _GuiEboSize = 2000;

    private int _FontTextureHandle;

    private Shader? _GuiShader;

    private int _WindowWidth;
    private int _WindowHeight;

    private readonly List<char> _PressedCharacters = new List<char>();

    /// <summary>
    /// Creates a new instance of the <see cref="ImGuiHelper"/> class.
    /// </summary>
    /// <param name="Width">The width of the viewport.</param>
    /// <param name="Height">The height of the viewport.</param>
    public ImGuiHelper(int Width, int Height)
    {
        _WindowWidth = Width;
        _WindowHeight = Height;

        IntPtr guiContext = ImGui.CreateContext();
        ImGui.SetCurrentContext(guiContext);

        ImGuiIOPtr guiIo = ImGui.GetIO();
        guiIo.Fonts.AddFontDefault();

        guiIo.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        
        CreateResources();
        SetKeyMappings();

        SetPerFrameImGuiData(1f / 60f);
        
        ImGui.NewFrame();
        _FrameBegunRendering = true;
    }

    /// <summary>
    /// Sets the internal viewport width and height used by ImGui.
    /// </summary>
    /// <param name="NewWidth">The new viewport width.</param>
    /// <param name="NewHeight">The new viewport height.</param>
    public void Resize(int NewWidth, int NewHeight)
    {
        _WindowWidth = NewWidth;
        _WindowHeight = NewHeight;
    }

    /// <summary>
    /// Creates all the resources needed for ImGui to function.
    /// <remarks>
    /// Doesn't affect the current OpenGL state before call.
    /// </remarks>
    /// </summary>
    public void CreateResources()
    {
        int previousVao = GL.GetInteger(GetPName.VertexArrayBinding);
        int previousVbo = GL.GetInteger(GetPName.ArrayBufferBinding);

        _GuiVao = GL.GenVertexArray();
        GL.BindVertexArray(_GuiVao);

        _GuiVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _GuiVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _GuiVboSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        _GuiEbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _GuiEbo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _GuiEboSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        
        RecreateFontTexture();

        _GuiShader = new Shader("imgui.vert", "imgui.frag");

        int stride = Unsafe.SizeOf<ImDrawVert>();
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);

        GL.BindVertexArray(previousVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, previousVbo);
    }

    /// <summary>
    /// Recreates the font texture handle to draw text.
    /// </summary>
    public void RecreateFontTexture()
    {
        ImGuiIOPtr guiIo = ImGui.GetIO();

        IntPtr pixels;
        int width;
        int height;
        
        guiIo.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height, out _);
        
        int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));
        int previousActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
        
        GL.ActiveTexture(TextureUnit.Texture0);
        
        int previousTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

        _FontTextureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _FontTextureHandle);
        GL.TexStorage2D(TextureTarget2d.Texture2D, mips, SizedInternalFormat.Rgba8, width, height);
        
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mips - 1);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        // Restore state
        GL.BindTexture(TextureTarget.Texture2D, previousTexture2D);
        GL.ActiveTexture((TextureUnit)previousActiveTexture);

        guiIo.Fonts.SetTexID(_FontTextureHandle);

        guiIo.Fonts.ClearTexData();
    }
    
    /// <summary>
    /// Renders the ImGui draw list data.
    /// </summary>
    public void Render()
    {
        if (_FrameBegunRendering)
        {
            _FrameBegunRendering = false;
            
            ImGui.Render();
            
            RenderImDrawData(ImGui.GetDrawData());
        }
    }

    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    /// <param name="Window">The target window.</param>
    /// <param name="DeltaTime">The time passed since the last frame.</param>
    public void Update(GameWindow Window, float DeltaTime)
    {
        if (_FrameBegunRendering)
        {
            ImGui.Render();
        }

        SetPerFrameImGuiData(DeltaTime);
        UpdateImGuiInput(Window);

        _FrameBegunRendering = true;
        
        ImGui.NewFrame();
    }

    /// <summary>
    /// Checks if ImGui wants keyboard focus.
    /// </summary>
    /// <returns>True if ImGui wants keyboard focus.</returns>
    public bool WantsKeyboardInput()
    {
        ImGuiIOPtr guiIo = ImGui.GetIO();
        return guiIo.WantCaptureKeyboard || guiIo.WantTextInput;
    }
    
    /// <summary>
    /// Sets the per-frame data needed.
    /// </summary>
    /// <param name="DeltaTime">The time passed since the last frame.</param>
    private void SetPerFrameImGuiData(float DeltaTime)
    {
        ImGuiIOPtr guiIo = ImGui.GetIO();
        
        guiIo.DisplaySize = new Vector2(_WindowWidth / 1f, _WindowHeight / 1f);
        
        guiIo.DisplayFramebufferScale = Vector2.One;
        guiIo.DeltaTime = DeltaTime;
    }

    /// <summary>
    /// Updates the ImGui input state.
    /// </summary>
    /// <param name="Window">The target window.</param>
    private void UpdateImGuiInput(GameWindow Window)
    {
        ImGuiIOPtr guiIo = ImGui.GetIO();

        MouseState mouseState = Window.MouseState;
        KeyboardState keyboardState = Window.KeyboardState;

        guiIo.MouseDown[0] = mouseState[MouseButton.Left];
        guiIo.MouseDown[1] = mouseState[MouseButton.Right];
        guiIo.MouseDown[2] = mouseState[MouseButton.Middle];

        Vector2i screenPoint = new Vector2i((int)mouseState.X, (int)mouseState.Y);
        Vector2i point = screenPoint;
        
        guiIo.MousePos = new Vector2(point.X, point.Y);

        if (!guiIo.WantCaptureKeyboard && !guiIo.WantTextInput) return;
            
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            if (key == Keys.Unknown) continue;
            
            guiIo.KeysDown[(int)key] = keyboardState.IsKeyDown(key);
        }
        foreach (char pressedCharacter in _PressedCharacters)
        {
            guiIo.AddInputCharacter(pressedCharacter);
        }
        
        _PressedCharacters.Clear();

        guiIo.KeyCtrl = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
        guiIo.KeyAlt = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
        guiIo.KeyShift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
        guiIo.KeySuper = keyboardState.IsKeyDown(Keys.LeftSuper) || keyboardState.IsKeyDown(Keys.RightSuper);
    }
    
    /// <summary>
    /// Marks a character as pressed for ImGui.
    /// </summary>
    /// <param name="Character">The pressed character.</param>
    internal void PressChar(char Character)
    {
        _PressedCharacters.Add(Character);
    }
    
    /// <summary>
    /// Sets mouse scroll offset data for ImGui.
    /// </summary>
    /// <param name="Offset">How much the scroll wheel has moved.</param>
    internal void MouseScroll(OpenTK.Mathematics.Vector2 Offset)
    {
        ImGuiIOPtr guiIo = ImGui.GetIO();
            
        guiIo.MouseWheel = Offset.Y;
        guiIo.MouseWheelH = Offset.X;
    }
    
    /// <summary>
    /// Sets the general key mappings.
    /// </summary>
    private static void SetKeyMappings()
    {
        ImGuiIOPtr guiIo = ImGui.GetIO();
        
        guiIo.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
        guiIo.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
        guiIo.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
        guiIo.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
        guiIo.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
        guiIo.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
        guiIo.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
        guiIo.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
        guiIo.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
        guiIo.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
        guiIo.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
        guiIo.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
        guiIo.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
        guiIo.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
        guiIo.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
        guiIo.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
        guiIo.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
        guiIo.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
        guiIo.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
    }
    
    /// <summary>
    /// Renders all the ImGui commands.
    /// </summary>
    /// <param name="DrawData">The ImGui draw data.</param>
    /// <exception cref="NotImplementedException">
    /// Thrown if a command has a user callback.
    /// </exception>
    private void RenderImDrawData(ImDrawDataPtr DrawData)
    {
        if (DrawData.CmdListsCount == 0) return;

        // Get intial state.
        int previousVao = GL.GetInteger(GetPName.VertexArrayBinding);
        int previousArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
        int previousProgram = GL.GetInteger(GetPName.CurrentProgram);
        bool previousBlendEnabled = GL.GetBoolean(GetPName.Blend);
        bool previousScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
        int previousBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
        int previousBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
        int previousBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
        int previousBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
        int previousBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
        int previousBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
        bool previousCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
        bool previousDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
        int previousActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
        
        GL.ActiveTexture(TextureUnit.Texture0);
        
        int previousTexture2D = GL.GetInteger(GetPName.TextureBinding2D);
        Span<int> previousScissorBox = stackalloc int[4];
        
        unsafe
        {
            fixed (int* scissorBoxPointer = &previousScissorBox[0])
            {
                GL.GetInteger(GetPName.ScissorBox, scissorBoxPointer);
            }
        }

        // Bind the element buffer (through the VAO) so that we can resize it.
        GL.BindVertexArray(_GuiVao);
        
        // Bind the vertex buffer so that we can resize it.
        GL.BindBuffer(BufferTarget.ArrayBuffer, _GuiVbo);
        
        for (int i = 0; i < DrawData.CmdListsCount; i++)
        {
            ImDrawListPtr commandList = DrawData.CmdListsRange[i];

            int vertexSize = commandList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _GuiVboSize)
            {
                int newSize = (int)Math.Max(_GuiVboSize * 1.5f, vertexSize);
                    
                GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _GuiVboSize = newSize;
            }

            int indexSize = commandList.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _GuiEboSize)
            {
                int newSize = (int)Math.Max(_GuiEboSize * 1.5f, indexSize);
                
                GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                
                _GuiEboSize = newSize;
            }
        }

        // Setup orthographic projection matrix into our constant buffer
        ImGuiIOPtr guiIo = ImGui.GetIO();
        Matrix4 mvpMatrix = Matrix4.CreateOrthographicOffCenter(0.0f, guiIo.DisplaySize.X, guiIo.DisplaySize.Y,
            0.0f, -1.0f, 1.0f);

        _GuiShader!.Use();
        _GuiShader!.SetUniform("projection_matrix", mvpMatrix);
        _GuiShader!.SetUniform("in_fontTexture", 0);

        GL.BindVertexArray(_GuiVao);

        DrawData.ScaleClipRects(guiIo.DisplayFramebufferScale);

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.ScissorTest);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        // Render command lists
        for (int n = 0; n < DrawData.CmdListsCount; n++)
        {
            ImDrawListPtr commandList = DrawData.CmdListsRange[n];

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, commandList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), commandList.VtxBuffer.Data);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, commandList.IdxBuffer.Size * sizeof(ushort), commandList.IdxBuffer.Data);

            for (int i = 0; i < commandList.CmdBuffer.Size; i++)
            {
                ImDrawCmdPtr commandPointer = commandList.CmdBuffer[i];
                
                if (commandPointer.UserCallback != IntPtr.Zero)
                {
                    throw new NotImplementedException();
                }
                
                else
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, (int)commandPointer.TextureId);

                    // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                    Vector4 clip = commandPointer.ClipRect;
                    GL.Scissor((int)clip.X, _WindowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));

                    if ((guiIo.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                    {
                        GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)commandPointer.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(commandPointer.IdxOffset * sizeof(ushort)), unchecked((int)commandPointer.VtxOffset));
                    }
                    else
                    {
                        GL.DrawElements(BeginMode.Triangles, (int)commandPointer.ElemCount, DrawElementsType.UnsignedShort, (int)commandPointer.IdxOffset * sizeof(ushort));
                    }
                }
            }
        }

        GL.Disable(EnableCap.Blend);
        GL.Disable(EnableCap.ScissorTest);

        // Reset state
        GL.BindTexture(TextureTarget.Texture2D, previousTexture2D);
        GL.ActiveTexture((TextureUnit)previousActiveTexture);
        GL.UseProgram(previousProgram);
        GL.BindVertexArray(previousVao);
        GL.Scissor(previousScissorBox[0], previousScissorBox[1], previousScissorBox[2], previousScissorBox[3]);
        GL.BindBuffer(BufferTarget.ArrayBuffer, previousArrayBuffer);
        GL.BlendEquationSeparate((BlendEquationMode)previousBlendEquationRgb, (BlendEquationMode)previousBlendEquationAlpha);
        GL.BlendFuncSeparate((BlendingFactorSrc)previousBlendFuncSrcRgb, (BlendingFactorDest)previousBlendFuncDstRgb,
            (BlendingFactorSrc)previousBlendFuncSrcAlpha, (BlendingFactorDest)previousBlendFuncDstAlpha);
        
        if (previousBlendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
        if (previousDepthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
        if (previousCullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
        if (previousScissorTestEnabled) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
    }
    
    /// <summary>
    /// Disposes all the OpenGL objects that need it.
    /// </summary>
    public void Dispose()
    {
        GL.DeleteVertexArray(_GuiVao);
        GL.DeleteBuffer(_GuiVbo);
        GL.DeleteBuffer(_GuiEbo);

        GL.DeleteTexture(_FontTextureHandle);
        _GuiShader?.Dispose();
    }
}