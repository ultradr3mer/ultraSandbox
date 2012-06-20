#version 130

precision highp float;

uniform vec3 in_light;

in vec3 in_position;
in vec2 in_texture;

out vec2 v_texture;
out vec3 v_view;

uniform mat4 invMVPMatrix;

void main(void)
{
	gl_Position = vec4(in_position, 1);
  
	v_texture = 1-in_texture;
	
	vec4 g_pos = vec4(in_position.xy,-1,1);
	g_pos = invMVPMatrix * g_pos;
	g_pos /= g_pos.w;
	
	vec4 g_pos2 = vec4(in_position.xy,1,1);
	g_pos2 = invMVPMatrix * g_pos;
	g_pos2 /= g_pos.w;
	
	v_view = normalize(g_pos2.xyz - g_pos.xyz);
} 