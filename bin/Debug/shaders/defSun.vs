#version 130

precision lowp float;

uniform vec3 in_light;

uniform mat4 modelview_matrix;

in vec3 in_position;

void main(void)
{
	gl_Position = vec4(in_position, 1);
}