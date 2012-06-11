#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;
uniform vec3 in_lightdir;

uniform vec3 in_eyepos;
uniform float in_near;
uniform float in_far;

in vec4 g_pos;
in vec3 v_ss_normal;
in vec2 v_texture;
in vec3 light;
in float v_depth;

uniform sampler2D baseTexture;
uniform sampler2D normalTexture;

out vec4 out_frag_color;

void main(void)
{	
	vec4 texN = vec4(v_ss_normal,1) * 0.5 + 0.5;
	
	out_frag_color = texN;
	
	out_frag_color.a = v_depth;//(in_far-in_near);//1-length(g_pos.xyz-in_eyepos)/(in_far-in_near);
}