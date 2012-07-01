#version 130

precision lowp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;
uniform vec3 in_lightdir;

uniform vec3 in_eyepos;
uniform float in_specexp;

in vec4 g_pos;
in vec3 v_normal;
in vec2 v_texture;
in vec3 light;
in float v_depth;
in float v_w;

uniform sampler2D baseTexture;
uniform sampler2D normalTexture;

out vec4 out_frag_color;

void main(void)
{	
	vec3 N = normalize(v_normal);

	out_frag_color.rgb = N * 0.5 + 0.5;
	
	out_frag_color.a = 0.5;
}