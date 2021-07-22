#version 330 core
layout (location = 0) in vec2 aPosition;
uniform float thickness;
uniform vec3 color;
uniform vec2 viewPortSize;

out vec4 brushColor;

void main()
{
    float x = (aPosition.x - viewPortSize.x/2)/viewPortSize.x/2;
    float y = -(aPosition.y - viewPortSize.y/2)/viewPortSize.y/2;
    gl_Position = vec4(aPosition, 0.0, 1.0);
    gl_PointSize = thickness;
    brushColor = vec4(color, 1.0);
}