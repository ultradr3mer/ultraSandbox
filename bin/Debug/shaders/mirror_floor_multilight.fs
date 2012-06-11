#version 130

precision lowp float;

uniform sampler2D reflectionTexture;

#variables

#functions


void main(void)
{
	vec4 NTexValue = texture(normalTexture, v_texture) * 2.0 - 1.0;
	vec3 N = v_normal;
	
	#include base.snip

	#include lighting.snip

	vec4 color = vec4(0.5,0.7,1.0,1.0);
	vec4 reflection = texture(reflectionTexture, screenposition);
	out_frag_color = vec4(all_lights, 1.0) + reflection * 0.2;
	out_frag_color *= base;
	out_frag_color[3] = 1;
	//out_frag_color = vec4(0.5,0.7,1.0,1.0)*light_angular;
	//out_frag_color = reflection;
}