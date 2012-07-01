#version 130

precision lowp float;

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
	
	vec4 g_pos_near = vec4(in_position.xy,-1,1);
	g_pos_near = invMVPMatrix * g_pos_near;
	g_pos_near /= g_pos_near.w;
		
	vec4 g_pos_far = vec4(in_position.xy,1,1);
	g_pos_far = invMVPMatrix * g_pos_far;
	g_pos_far /= g_pos_far.w;
	
	v_view = normalize(g_pos_far.xyz - g_pos_near.xyz);
} 