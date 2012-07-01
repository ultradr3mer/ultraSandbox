#version 130

#variables

#functions

in vec3 viewDir;

uniform float fresnelExp;
uniform float fresnelStr;

float PI = 3.14159265;
int samples = 5; //samples on the first ring (5-10)
int rings = 2; //ring count (2-6)

vec4 getback(vec2 coord,float blur_size) {
	int i;
	vec4 base_color = vec4(0.0,0.0,0.0,0.0);
	
	//vec2 rnd = texture(Texture2,gl_FragCoord.xy/128).xy * 2 -1;
	float step = PI*2.0 / float(samples);

	float pw;
	float ph;
	float ringcoord;
	
	vec2 size = blur_size/in_rendersize/rings;
	
	vec4 col;	
	float s = 0;
	float aoscale = 0.02;
	
	for (int i = 1 ; i <= rings; ++i)
	{
		for (int j = 0 ; j < samples; ++j)
		{	
			ringcoord = float(j)*step;
			pw = cos(ringcoord)*i;
			ph = sin(ringcoord)*i;
			col += texture(backColorTexture, vec2(coord.s+pw*size.s,coord.t+ph*size.t) );
		}
	}
	
	return col/samples/rings;
}

void main(void)
{
	#include base.snip
	
	#include defReflections.snip
		
	vec3 emit = vec3(0,0,0);
	if(use_emit){
		emit = in_emitcolor;
		emit *= info.g;
	}
	
	#include defLighting.snip
	
	float fresnel = 1;
	if(fresnelExp > 0)
	{
		fresnel = clamp(dot(viewDir,v_normal)+1.0,0.0,1.0);
		fresnel = pow(fresnel,fresnelExp);
		fresnel *= fresnelStr;
		fresnel += 1;
	}
	
	float lightBrightness =  3.0 / (all_lights.r + all_lights.g + all_lights.b);

	out_frag_color.rgb = all_lights*base.rgb+all_spec*all_lights*lightBrightness*fresnel+env+emit;
	out_frag_color.a = 1.0;
	
	//out_frag_color.rgb = vec3(0.2,0.2,0.2)*fresnel;
	
  //out_frag_color = vec4(all_spec,1);
  //out_frag_color = texture(EnvTexture1,vec2(refn.x,refn.y));
}