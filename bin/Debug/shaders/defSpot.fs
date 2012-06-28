#version 130

#variables

#functions

void main() 
{
#include defBase.snip
	
	vec3 lightDirection = normalize(defPosition - g_pos);

	float angle = clamp(dot(lightDirection, N),0,1);

	vec3 refn  = reflect(viewVec,N);
	float spec = clamp(dot(lightDirection, refn),0,1);
	spec = pow(spec,Ntex.a*Ntex.a*250.0);
	
	float bias = 0.008/clamp(angle,0.01,1.0);
	float shadow = calcShadow(vec4(g_pos,1), rnd, bias);

	out_frag_color.rgb = defColor*angle*shadow;
	out_frag_color.a = spec*shadow;
}