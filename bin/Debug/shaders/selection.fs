#version 130

precision lowp float;

uniform float selected;

in vec3 v_ss_normal;
in vec2 v_texture;
in vec3 v_ss_tangent;
in vec3 v_ss_bnormal;

uniform sampler2D normalTexture;

out vec4 out_frag_color;

void main(void)
{
	vec4 NTexValue = texture(normalTexture, v_texture) * 2.0 - 1.0;
	vec3 N = normalize(NTexValue[2] * v_ss_normal + NTexValue[0] * v_ss_tangent + NTexValue[1] * v_ss_bnormal);
	
	out_frag_color = vec4(1,1,1,1)*dot(vec3(0,0,1),N.xyz)*1.5*selected;
	out_frag_color.a = 1;
}