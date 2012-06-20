#version 130

#variables

#functions

void main(void)
{
	#include vAnimationNormals.snip
	
	#include vBase.snip replace:in_position:ani_position replace:in_normal:ani_normal replace:in_tangent:ani_tangent
	
	gl_Position.z += 0.0001;
}