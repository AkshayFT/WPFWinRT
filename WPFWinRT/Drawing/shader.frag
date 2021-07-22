#version 330 core
precision highp float;
precision highp int;

uniform sampler2D texture0;
varying vec4 destinationColor;

void main()
{
	vec4 col = vec4(1.0, 0.0, 0.0, 1.0);
	gl_FragColor = texture2D(texture0, gl_PointCoord) * col;
}