#version 130

precision lowp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;
uniform vec3 in_lightdir;

uniform vec3 in_eyepos;
uniform float in_near;
uniform float in_far;

in vec4 g_pos;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;
in vec3 light;
in float v_depth;
in float v_w;

uniform sampler2D baseTexture;
uniform sampler2D normalTexture;

out vec4 out_frag_color;

void main(void)
{
	vec4 NTexValue = texture(normalTexture, v_texture);
	vec3 N = NTexValue.agb * 2.0 - 1.0;
	N = normalize(N[2] * v_normal + N[0] * v_tangent + N[1] * v_bnormal);
	
	out_frag_color.rgb = N * 0.5 + 0.5;
	
	out_frag_color.a = NTexValue.r;//(in_far-in_near);//1-length(g_pos.xyz-in_eyepos)/(in_far-in_near);
}