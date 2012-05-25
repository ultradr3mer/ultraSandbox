#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;
uniform vec3 in_lightdir;

uniform float selected;

const int no_lights = 6;

uniform vec3 in_n_light[no_lights];
uniform vec3 in_n_lightdir[no_lights];
uniform vec3 in_n_lightcolor[no_lights];

in vec4 g_pos;
in vec3 v_eyedirection;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;

uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform sampler2D Texture3;

uniform sampler2D EnvTexture1;
uniform sampler2D EnvTexture2;
uniform sampler2D EnvTexture3;
uniform sampler2D EnvTexture4;
uniform sampler2D EnvTexture5;
uniform sampler2D EnvTexture6;

out vec4 out_frag_color;

void main(void)
{
	vec4 NTexValue = texture(Texture1, v_texture) * 2.0 - 1.0;
	vec3 N = normalize(NTexValue[2] * v_normal + NTexValue[0] * v_tangent + NTexValue[1] * v_bnormal);
	
	out_frag_color = vec4(1,1,1,1)*dot(vec3(0,0,1),N.xyz)*1.5*selected;
	out_frag_color.a = 1;
}