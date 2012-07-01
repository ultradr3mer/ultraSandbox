#version 130

precision highp float;

uniform vec3 in_light;

in vec3 in_position;
in vec2 in_texture;

out vec2 v_texture;

void main(void)
{
	gl_Position = vec4(in_position, 1);
  
	v_texture = 1-in_texture;
}