#version 330 core

uniform mat4 projection_matrix;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 Color;
out vec2 TexCoord;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    Color = in_color;
    TexCoord = in_texCoord;
}