#version 130

precision lowp float;

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

uniform mat4 invMVPMatrix;

out vec3 g_pos_far;

void main(void)
{
	vec4 g_pos = vec4(in_position.xyz,1);
	g_pos = defInvPMatrix * g_pos;
	g_pos.xyz /= g_pos.w;
	g_pos.w = 1;
	
	vec4 screenpos = projection_matrix * modelview_matrix * g_pos;
	gl_Position = screenpos;
	
	screenpos /= screenpos.w;

	g_pos = invMVPMatrix * vec4(screenpos.xy,1,1);
	g_pos /= g_pos.w;
	
	g_pos_far = g_pos.xyz;
	//g_pos_far = vec3(screenpos.xy,0);
}