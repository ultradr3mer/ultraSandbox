#version 130

precision highp float;

uniform vec3 in_lightambient;

const int no_lights = 6;

uniform vec3 in_n_light[no_lights];
uniform vec3 in_n_lightdir[no_lights];
uniform vec3 in_n_lightcolor[no_lights];

in vec4 g_pos;
in vec3 v_eyedirection;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;
in vec3 light;

in float v_depth;

in vec3 v_ss_normal;
in vec3 v_ss_tangent;
in vec3 v_ss_bnormal;

uniform sampler2D baseTexture;
uniform sampler2D normalTexture;
uniform sampler2D emitTexture;

uniform sampler2D backColorTexture;
uniform sampler2D backDepthTexture;

uniform sampler2D EnvTexture1;
uniform sampler2D EnvTexture2;
uniform sampler2D EnvTexture3;
uniform sampler2D EnvTexture4;
uniform sampler2D EnvTexture5;
uniform sampler2D EnvTexture6;

out vec4 out_frag_color;

uniform bool use_env;
uniform bool env_a_base;
uniform bool env_a_normal;
uniform vec3 env_tint;

uniform bool use_emit;
uniform bool emit_a_base;
uniform bool emit_a_normal;
uniform vec3 in_emitcolor;

uniform bool use_alpha;
uniform vec2 in_rendersize;

uniform float ref_size;
uniform float blur_size;
uniform float fresnel_str;

vec2 screenpos(){
	return gl_FragCoord.xy/in_rendersize;
}

vec4 get_env(vec3 refn){
	float env_strenth = 1;

	vec3 tmpn = refn;
	
	refn.x = -refn.x;

	tmpn.x *= sign(tmpn.x);
	tmpn.y *= sign(tmpn.y);
	tmpn.z *= sign(tmpn.z);
		
	vec4 env = vec4(0,0,0,1);
	if(tmpn.x > tmpn.y && tmpn.x > tmpn.z){
		if(refn.x < 0){
			vec2 envuv = vec2(refn.z/refn.x,refn.y/refn.x)*0.5+0.5;
			return texture(EnvTexture3,envuv)*env_strenth;
		} else {
			vec2 envuv = vec2(refn.z/refn.x,-refn.y/refn.x)*0.5+0.5;
			return texture(EnvTexture1,envuv)*env_strenth;
		}
	} 
	if(tmpn.z > tmpn.y && tmpn.z > tmpn.x){
		if(refn.z < 0){
			vec2 envuv = vec2(-refn.x/refn.z,refn.y/refn.z)*0.5+0.5;
			return texture(EnvTexture4,envuv)*env_strenth;
		} else {
			vec2 envuv = vec2(-refn.x/refn.z,-refn.y/refn.z)*0.5+0.5;
			return texture(EnvTexture2,envuv)*env_strenth;
		}
	}
	if(tmpn.y > tmpn.z && tmpn.y > tmpn.x){
		if(refn.y < 0){
			vec2 envuv = vec2(-refn.x/refn.y,-refn.z/refn.y)*0.5+0.5;
			return texture(EnvTexture6,envuv)*env_strenth;
		} else {
			vec2 envuv = vec2(refn.x/refn.y,-refn.z/refn.y)*0.5+0.5;
			return texture(EnvTexture5,envuv)*env_strenth;
		}
	} 
}

vec3 getspec(vec3 refn,vec3 light,int i){
	
	float specular_strenth = 0.6;
	float specular_exponent = 10;
	
	vec3 light_vec = normalize(in_n_light[i]-g_pos.xyz);
	vec3 eye_dir = refn;
	
	float specular = clamp(dot(light_vec, eye_dir), 0.0, 1.0);
	
	float final_spac = pow(specular,specular_exponent)*specular_strenth;
	
	return final_spac*light;
	
}

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

vec3 getlight(vec3 N,int i){
	float brightness = 2;

	float ang_hardness = 10;
	float ang_limit = 0.7;
	float lenth_mult = 0.2;
	
	vec3 refn = reflect(v_eyedirection,N);
	
	vec3 specular_final = vec3(0,0,0);
		

			vec3 light_vec = normalize(in_n_light[i]-g_pos.xyz);
			vec3 light_dir = normalize(in_n_lightdir[i]);
			
			float light_angular = clamp((dot(light_vec,-light_dir)-ang_limit)*ang_hardness, 0.0, 1.0);
			float light_diffuse = clamp(dot(light_vec, N), 0.0, 1.0);
			float light_distance = 1/exp2(length(in_n_light[i]-g_pos.xyz)*lenth_mult);
			
						
			return light_angular*light_diffuse*light_distance*brightness*in_n_lightcolor[i];
}

void main(void)
{
	float depth = gl_FragCoord.z;
	vec2 screenposition = screenpos();
	if(use_alpha && (depth > texture(backDepthTexture, screenposition).x)){
		discard;
	}
	vec4 base = texture(baseTexture, v_texture);

	vec4 NTexValue = texture(normalTexture, v_texture) * vec4(2,2,2,1) - vec4(1,1,1,0);
	vec3 N = normalize(NTexValue[2] * v_normal + NTexValue[0] * v_tangent + NTexValue[1] * v_bnormal);
	vec3 SSN = normalize(NTexValue[2] * v_ss_normal + NTexValue[0] * v_ss_tangent + NTexValue[1] * v_ss_bnormal);
	
	vec3 refn = reflect(v_eyedirection,N);
	
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
	
		if(env_a_base)
			emit *= base.a;
			
		if(env_a_normal)
			emit *= NTexValue.a;
	}
	
	float alpha = 1;

	vec3 all_lights = vec3(0,0,0);
	vec3 all_spec = vec3(0,0,0);
	
	for(int i = 0; i < no_lights; i++) {
		if(length(in_n_lightcolor[i]) > 0){
				vec3 light = getlight(N,i);
				vec3 spec = getspec(refn,light,i);
				all_lights += light;
				all_spec += spec;
			}
	}

	float diffuse = clamp(dot(light, N), 0.0, 1.0);
	out_frag_color = vec4(in_lightambient + all_lights, 1.0)*base+vec4(all_spec,1)+env*NTexValue.a+emit;
	  
	if(use_alpha){
		float alpha = base.a;
		if(fresnel_str != 0){
			alpha = clamp(alpha-dot(vec3(0,0,1),SSN.xyz)*fresnel_str, 0.0, 1.0);
		}
		if(ref_size != 0){
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
	
  //out_frag_color = env/0.2;
  //out_frag_color = vec4(all_spec,1);
  //out_frag_color = texture(EnvTexture1,vec2(refn.x,refn.y));
}