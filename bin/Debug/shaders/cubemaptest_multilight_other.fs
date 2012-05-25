#version 130

precision lowp float;

uniform vec3 in_lightambient;

const int no_lights = 10;

uniform int in_no_lights;

struct Light
{
    bool active;
    vec3 position;
	vec3 direction;
    vec3 color;
	mat4 view_matrix;
	bool texture;
};

struct SunLight
{
    bool active;
	vec3 direction;
    vec3 color;
	mat4 view_matrix;
};

uniform Light lightStructs[no_lights];
uniform SunLight sunLightStruct;

in vec4 g_pos;
in vec3 v_eyedirection;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;

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

in vec4 v_s_map_pos[no_lights];
uniform sampler2D shadowTexture;
uniform sampler2D sunShadowTexture;
uniform sampler2D noiseTexture;

uniform bool use_env;
uniform bool env_a_base;
uniform bool env_a_normal;
uniform vec3 env_tint;

uniform float shadow_quality;

in vec4 v_sun_map_pos;

uniform bool use_spec;
uniform bool spec_a_base;
uniform bool spec_a_normal;
uniform vec3 in_speccolor;
uniform float in_specexp;

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
	vec3 tmpn = refn;
	
	refn.x = -refn.x;

	tmpn.x *= sign(tmpn.x);
	tmpn.y *= sign(tmpn.y);
	tmpn.z *= sign(tmpn.z);
		
	vec4 env = vec4(0,0,0,1);
	if(tmpn.x > tmpn.y && tmpn.x > tmpn.z){
		if(refn.x < 0){
			vec2 envuv = vec2(refn.z/refn.x,refn.y/refn.x)*0.5+0.5;
			return texture(EnvTexture3,envuv);
		} else {
			vec2 envuv = vec2(refn.z/refn.x,-refn.y/refn.x)*0.5+0.5;
			return texture(EnvTexture1,envuv);
		}
	} 
	if(tmpn.z > tmpn.y && tmpn.z > tmpn.x){
		if(refn.z < 0){
			vec2 envuv = vec2(-refn.x/refn.z,refn.y/refn.z)*0.5+0.5;
			return texture(EnvTexture4,envuv);
		} else {
			vec2 envuv = vec2(-refn.x/refn.z,-refn.y/refn.z)*0.5+0.5;
			return texture(EnvTexture2,envuv);
		}
	}
	if(tmpn.y > tmpn.z && tmpn.y > tmpn.x){
		if(refn.y < 0){
			vec2 envuv = vec2(-refn.x/refn.y,-refn.z/refn.y)*0.5+0.5;
			return texture(EnvTexture6,envuv);
		} else {
			vec2 envuv = vec2(refn.x/refn.y,-refn.z/refn.y)*0.5+0.5;
			return texture(EnvTexture5,envuv);
		}
	} 
}

float getspec(vec3 refn,int i)
{	
	vec3 light_vec = normalize(lightStructs[i].position - g_pos.xyz);
	vec3 eye_dir = refn;
	
	float specular = clamp(dot(light_vec, eye_dir), 0.0, 1.0);
	
	float final_spec = pow(specular,in_specexp);
	
	return final_spec;
	
}

float getSunSpec(vec3 refn)
{	
	vec3 light_vec = -sunLightStruct.direction;
	vec3 eye_dir = refn;
	
	float specular = clamp(dot(light_vec, eye_dir), 0.0, 1.0);
	
	float final_spec = pow(specular,in_specexp);
	
	return final_spec;
	
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

float shadowSample(vec3 coord){
	float distanceFromLight = texture(shadowTexture ,coord.st).a + 0.005;	
	return distanceFromLight < coord.z ? 0.0 : 1.0 ;
}

float sunShadowSample(vec3 coord){
	float distanceFromLight = texture(sunShadowTexture ,coord.st).z + 0.005;	
	return distanceFromLight < coord.z ? 0.0 : 1.0 ;
}

vec3 getlight(vec3 N,int number,vec2 rnd){	
		vec3 light_final = vec3(0,0,0);
		
		vec4 shadowCoordinateWdivide = v_s_map_pos[number] / v_s_map_pos[number].w ;

		if(0 < shadowCoordinateWdivide.x 
		&& 1 > shadowCoordinateWdivide.x 
		&& 0 < shadowCoordinateWdivide.y 
		&& 1 > shadowCoordinateWdivide.y 
		&& shadowCoordinateWdivide.z > 0
		&& shadowCoordinateWdivide.z < 1)
		{
			float radius = 0.005;
			
			float brightness = 10;

			float ang_hardness = 10;
			float ang_limit = 0.7;
			float lenth_mult = 0.2;
		
			shadowCoordinateWdivide.x = (shadowCoordinateWdivide.x + number) / float(in_no_lights);
			
			vec3 light_vec = normalize(lightStructs[number].position-g_pos.xyz);
			vec3 light_dir = normalize(lightStructs[number].direction);
			
			float light_diffuse = clamp(dot(light_vec, N), 0.0, 1.0);
			
			if(light_diffuse < 0.01)
				return vec3(0,0,0);
				
			vec3 light_angular;
			
			if(lightStructs[number].texture)
			{
				light_angular = texture(shadowTexture ,shadowCoordinateWdivide.st).rgb;
			}
			else
			{
				light_angular = vec3(1,1,1)*clamp((dot(light_vec,-light_dir)-ang_limit)*ang_hardness, 0.0, 1.0);
			}
			
			if(light_angular.r < 0.01 && light_angular.g < 0.01 && light_angular.b < 0.01)
				return vec3(0,0,0);
			
			//float light_distance = 1/exp2(length(lightStructs[number].position-g_pos.xyz)*lenth_mult);
			float light_distance = pow(1-shadowCoordinateWdivide.z,1.3);
			
			float shadow = 0;
			
			float distanceFromLight = texture(shadowTexture ,shadowCoordinateWdivide.st).z;
			
			//shadowCoordinateWdivide.xy += rnd*vec2(radius/float(in_no_lights),radius)*0.5;
			
			if(shadow_quality <= 0.5){
				float x,y;		

				float checkerquad = radius*1.1;
				y = -checkerquad;
				x = -checkerquad;
				shadow += shadowSample(shadowCoordinateWdivide.xyz+vec3(x/float(in_no_lights),y,0));
				x = checkerquad;
				shadow += shadowSample(shadowCoordinateWdivide.xyz+vec3(x/float(in_no_lights),y,0));
				
				y = checkerquad;
				x = -checkerquad;
				shadow += shadowSample(shadowCoordinateWdivide.xyz+vec3(x/float(in_no_lights),y,0));
				x = checkerquad;
				shadow += shadowSample(shadowCoordinateWdivide.xyz+vec3(x/float(in_no_lights),y,0));
				
				int shadowsamples = 4;

				if(shadow != 0 && shadow != 4)
				{					
					float stepsize = radius*shadow_quality;
					for (y = -radius ; y <=radius ; y+=stepsize)
					{
						for (x = -radius ; x <=radius ; x+=stepsize)
						{
							shadow += shadowSample(shadowCoordinateWdivide.xyz+vec3(x/float(in_no_lights),y,0));
							shadowsamples++;
						}
					}
				}							
				shadow /= shadowsamples;
			} else {
				shadow = shadowSample(shadowCoordinateWdivide.xyz);
			}

			light_final += shadow*light_angular*light_diffuse*light_distance*brightness*lightStructs[number].color;

		}
	
	return light_final;
}

vec3 getSunLight(vec3 N, vec2 rnd){
	vec4 shadowCoordinateWdivide = v_sun_map_pos / v_sun_map_pos.w;
	float shadow = 1;
	
	float light_diffuse = clamp(dot(-sunLightStruct.direction, N), 0.0, 1.0);
	
	if(light_diffuse < 0.01)
		return vec3(0,0,0);
	
	if(0 < shadowCoordinateWdivide.x 
	&& 1 > shadowCoordinateWdivide.x 
	&& 0 < shadowCoordinateWdivide.y 
	&& 1 > shadowCoordinateWdivide.y 
	&& shadowCoordinateWdivide.z > 0
	&& shadowCoordinateWdivide.z < 1)
	{		
		float radius = 0.001;
		shadow = 0;

		shadowCoordinateWdivide.xy += rnd*vec2(1,1)*radius*0.7;
		
		if(shadow_quality > 0.1){
			float x,y;
			for (y = -shadow_quality ; y <=shadow_quality ; y+=1.0)
				for (x = -shadow_quality ; x <=shadow_quality ; x+=1.0)
					shadow += sunShadowSample(shadowCoordinateWdivide.xyz+vec3(x*radius,y*radius,0));
								
			shadow /= pow(shadow_quality*2+1,2);
		} else {
			shadow = sunShadowSample(shadowCoordinateWdivide.xyz);
		}
		
		//return sunLightStruct.color*sunShadowSample(shadowCoordinateWdivide.xyz);
		
	}

	return sunLightStruct.color * shadow * light_diffuse;
}

void main(void)
{
	float depth = gl_FragCoord.z;
	vec2 screenposition = screenpos();
	
	if(use_alpha && (depth > texture(backDepthTexture, screenposition).x)){
		discard;
	}
	
	vec3 light;
	
	vec4 base = texture(baseTexture, v_texture);
	vec2 rnd = texture(noiseTexture,gl_FragCoord.xy/128).xy * 2 -1;

	vec4 NTexValue = texture(normalTexture, v_texture) * vec4(2,2,2,1) - vec4(1,1,1,0);
	vec3 N = normalize(NTexValue[2] * v_normal + NTexValue[0] * v_tangent + NTexValue[1] * v_bnormal);
	
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
	
		if(emit_a_base)
			emit *= base.a;
			
		if(emit_a_normal)
			emit *= NTexValue.a;
	}
	
	float alpha = 1;

	vec3 all_lights = vec3(0,0,0);
	vec3 all_spec = vec3(0,0,0);

	light += getSunLight(N,rnd);
	all_lights += light;
	
	if(use_spec)
	{
		if(light.r > 0.01 || light.g > 0.01 || light.b > 0.01)
			all_spec += getSunSpec(refn) * light;
	}
	
	for(int i = 0; i < no_lights; i++) {
		if(i < in_no_lights){
			if(lightStructs[i].active)
			{
				//all_lights += vec3(0.3,0,0);
				light = getlight(N,i,rnd);
				all_lights += light;
				if(use_spec)
				{
					if(light.r > 0.01 || light.g > 0.01 || light.b > 0.01)
						all_spec += getspec(refn,i) * light;
				}
			}
		}
	}
	
	if(use_spec){	
		if(spec_a_base)
			all_spec *= base.a;
			
		if(spec_a_normal)
			all_spec *= NTexValue.a;
			
		if(in_speccolor != vec3(0,0,0))
			all_spec *= in_speccolor;
	}

	float diffuse = clamp(dot(light, N), 0.0, 1.0);
	out_frag_color = vec4(in_lightambient + all_lights, 1.0)*base+vec4(all_spec,1)+env*NTexValue.a+emit;
	  
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
	
  //out_frag_color = env/0.2;
  //out_frag_color = vec4(all_spec,1);
  //out_frag_color = texture(EnvTexture1,vec2(refn.x,refn.y));
}