#version 130
precision highp float;

in vec3 in_position;

void main(void)
{
	gl_Position = vec4(in_position, 1.0 );
}