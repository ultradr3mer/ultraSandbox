#version 130

#variables

out vec3 viewDir;

#functions

void main(void)
{
  //works only for orthogonal modelview
  //normal = normalize((modelview_matrix * vec4(in_normal, 0)).xyz);

	#include vBase.snip
	  		
	#include vLightCalc.snip
	
	viewDir = normalize(g_pos.xyz - in_eyepos);
}