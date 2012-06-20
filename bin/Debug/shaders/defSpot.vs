#version 130

precision highp float;

uniform vec3 in_light;

uniform mat4 modelview_matrix;
uniform mat4 projection_matrix;

in vec3 in_position;
in vec2 in_texture;

uniform vec3 defDirection;
uniform vec3 defColor;
uniform mat4 defMatrix;
uniform mat4 defInnerMatrix;
uniform mat4 defInvPMatrix;

out vec2 v_texture;

void main(void)
{
	vec4 g_pos = vec4(in_position.xyz,1);
	g_pos = defInvPMatrix * g_pos;
	g_pos /= g_pos.w;
	
	g_pos.w = 1;
	gl_Position = projection_matrix * modelview_matrix * g_pos;

	v_texture = 1-in_texture;
}