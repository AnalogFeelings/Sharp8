#version 330 core

uniform sampler2D in_fontTexture;

in vec4 Color;
in vec2 TexCoord;

out vec4 outputColor;

void main()
{
    outputColor = Color * texture(in_fontTexture, TexCoord);
}