#version 130

#variables

#functions

void main() 
{
#include defBase.snip
		
	float angle = clamp(dot(-defDirection, N),0,1);
	
	vec3 refn  = reflect(viewVec,N);
	float spec = clamp(dot(-defDirection, refn),0,1);
	spec = pow(spec,Ntex.a*Ntex.a*250.0);
	
	float bias = 0.001/clamp(angle,0.01,1.0);
	float shadow = calcSunShadow(vec4(g_pos,1), rnd, bias);
	
	out_frag_color.rgb = in_lightambient+defColor*angle*shadow;
	out_frag_color.a = spec*shadow*angle;
}