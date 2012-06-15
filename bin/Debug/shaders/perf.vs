#version 130
precision highp float;

in vec3 in_position;
uniform mat4 projection_matrix;

void main(void)
{
	gl_Position = normalize(projection_matrix * vec4(in_position,1.0));
}