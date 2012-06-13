#version 130

#variables

#functions

void main(void)
{
  	#include vAnimation.snip

	#include vBase.snip replace:in_position:ani_position replace:in_normal:ani_normal replace:in_tangent:ani_tangent
	
	if(use_alpha)
	{
		v_ss_normal = normalize((modelview_matrix * rotation_matrix * vec4(ani_normal, 0)).xyz);
		v_ss_tangent = normalize((modelview_matrix * rotation_matrix * vec4(ani_tangent, 0)).xyz);
		v_ss_bnormal = normalize(cross(v_ss_normal, v_ss_tangent));
	}
	
	#include vLightCalc.snip
}