#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in float thickness;


void main()
{
    gl_Position = vec4(aPosition, 0.0, 1.0);
    gl_PointSize = 20;
}