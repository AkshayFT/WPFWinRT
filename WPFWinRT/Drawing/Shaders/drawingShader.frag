#version 330 core

uniform sampler2D texture0;
in vec4 brushColor;

void main()
{
	gl_FragColor = texture2D(texture0, gl_PointCoord) * brushColor;
}