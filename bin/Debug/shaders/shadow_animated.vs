#version 130

#variables

#functions

uniform int curLight;

void main(void)
{
  //works only for orthogonal modelview
  //normal = normalize((modelview_matrix * vec4(in_normal, 0)).xyz);
		
	#include vAnimation.snip
	
	#include vBase.snip replace:in_position:ani_position

	vec4 shifted = gl_Position;
	
	if(shifted.w > 0){	
		shifted.xyz = shifted.xyz / shifted.w;
		shifted.x = (shifted.x+1+curLight*2)/in_no_lights-1;
		shifted.w = 1;
	}
		
	gl_Position = shifted;
}