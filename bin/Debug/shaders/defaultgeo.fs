#version 130

#variables

#functions

in vec3 heat;

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
	vec4 NTexValue = texture(normalTexture, v_texture) * vec4(2,2,2,1) - vec4(1,1,1,0);
	//vec3 N = normalize(NTexValue[2] * v_normal + NTexValue[0] * v_tangent + NTexValue[1] * v_bnormal);
	
	#include base.snip
	
	#include defReflections.snip
	
	
	vec3 emit = vec3(0,0,0);
	if(use_emit){
		emit = in_emitcolor;
	
		if(emit_a_base)
			emit *= base.a;
			
		if(emit_a_normal)
			emit *= NTexValue.a;
	}
	
	#include defLighting.snip

	out_frag_color.rgb = all_lights*base.rgb+all_spec+env+emit;
	out_frag_color.a = 1.0;

  //out_frag_color = vec4(all_spec,1);
  //out_frag_color = texture(EnvTexture1,vec2(refn.x,refn.y));
}