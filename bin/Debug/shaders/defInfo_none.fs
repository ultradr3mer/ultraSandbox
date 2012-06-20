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

out vec4 out_frag_color;

void main(void)
{
	out_frag_color.rgb = vec3(0,0,0);
	out_frag_color.a = v_depth/v_w;
}