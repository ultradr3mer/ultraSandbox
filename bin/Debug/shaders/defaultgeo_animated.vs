#version 130

#variables

out vec3 viewDir;

#functions

void main(void)
{
  	#include vAnimationNormals.snip

	#include vBase.snip replace:in_position:ani_position replace:in_normal:ani_normal replace:in_tangent:ani_tangent
		
	#include vLightCalc.snip
	
	viewDir = normalize(g_pos.xyz - in_eyepos);
}