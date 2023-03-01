#version 430 core

uniform sampler2D quadTexture;

in vec2 TexCoord;

out vec4 FragColor;

void main()
{
    //Here we sample the texture based on the Uv coordinates of the fragment
    FragColor = texture(quadTexture, TexCoord);
}