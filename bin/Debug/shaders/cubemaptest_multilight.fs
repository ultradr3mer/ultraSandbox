#version 130

#variables

#functions

#include reflections.snip

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
	vec3 N = normalize(NTexValue[2] * v_normal + NTexValue[0] * v_tangent + NTexValue[1] * v_bnormal);
	
	#include base.snip
	
	vec4 env = vec4(0,0,0,0);
	if(use_env){
		env = get_env(refn);
		
		if(env_a_base)
			env *= base.a;
			
		if(env_a_normal)
			env *= NTexValue.a;
		
		if(env_tint != vec3(0,0,0))
			env *= vec4(env_tint,1);
	}
	
	vec4 emit = vec4(0,0,0,0);
	if(use_emit){
		emit = vec4(in_emitcolor,0);
	
		if(emit_a_base)
			emit *= base.a;
			
		if(emit_a_normal)
			emit *= NTexValue.a;
	}
	
	float alpha = 1;

	#include lighting.snip

	float diffuse = clamp(dot(light, N), 0.0, 1.0);
	out_frag_color = vec4(all_lights, 1.0)*base+vec4(all_spec,1)+env*NTexValue.a+emit;
	  
	if(use_alpha){
		vec3 SSN = normalize(NTexValue[2] * v_ss_normal + NTexValue[0] * v_ss_tangent + NTexValue[1] * v_ss_bnormal);
		
		float alpha = base.a;
		if(fresnel_str != 0){
			alpha = clamp(alpha-dot(vec3(0,0,1),SSN.xyz)*fresnel_str, 0.0, 1.0);
		}
		if(ref_size > 0){
			vec2 scaling = ref_size*vec2(in_rendersize.y/in_rendersize.x,1);
			vec2 ray = -SSN.xy*scaling/v_depth;
			
			screenposition += ray;
		}
		vec4 back;
		
		if(blur_size != 0){
			back = getback(screenposition,blur_size/v_depth);
		} else {
			back = texture(backColorTexture,screenposition);
		}
		
		if(ref_size != 0 || blur_size != 0){
			out_frag_color = back*(1-alpha)+out_frag_color*alpha;
			//out_frag_color = vec4(1,1,1,1)*v_depth*0.1;
			out_frag_color.a = 1;
		} else {
			out_frag_color.a = alpha;
		}
	}
	
  out_frag_color += vec4(heat*0.5,0);
  //out_frag_color = vec4(all_spec,1);
  //out_frag_color = texture(EnvTexture1,vec2(refn.x,refn.y));
}