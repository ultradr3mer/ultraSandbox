#version 130

#variables

out vec3 v_ss_normal;
out vec3 v_ss_tangent;
out vec3 v_ss_bnormal;

#functions

void main(void)
{
	#include vBase.snip
	
	v_ss_normal = normalize((modelview_matrix * rotation_matrix * vec4(in_normal, 0)).xyz);
	v_ss_tangent = normalize((modelview_matrix * rotation_matrix * vec4(in_tangent, 0)).xyz);
	v_ss_bnormal = normalize(cross(v_ss_normal, v_ss_tangent));
}