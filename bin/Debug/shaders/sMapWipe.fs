#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;
uniform vec3 in_lightdir;

const int no_lights = 6;

uniform vec3 in_n_light[no_lights];
uniform vec3 in_n_lightdir[no_lights];
uniform vec3 in_n_lightcolor[no_lights];

uniform sampler2D Texture1;

uniform int curLight;
uniform vec2 in_rendersize;

in vec4 g_pos;
in vec3 v_eyedirection;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;

out vec4 out_frag_color;

void main(void)
{
	vec3 texValue = texture(Texture1,vec2(v_texture.x,1-v_texture.y)).rgb;
	out_frag_color = vec4(texValue,1);
}