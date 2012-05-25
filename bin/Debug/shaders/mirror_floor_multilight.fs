#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;
uniform vec3 in_lightdir;
uniform vec3 in_light;

uniform vec2 in_rendersize;

in vec4 g_pos;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;

uniform int in_no_lights;
float shadow_quality = 1;
uniform float sunradius = 0.001;

const int no_lights = 10;

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
	mat4 inner_view_matrix;
};

uniform Light lightStructs[no_lights];
uniform SunLight sunLightStruct;

in vec4 v_sun_map_pos;
in vec4 v_inner_sun_map_pos;

in vec4 v_s_map_pos[no_lights];

uniform sampler2D baseTexture;
uniform sampler2D normalTexture;
uniform sampler2D reflectionTexture;
uniform sampler2D noiseTexture;

uniform sampler2D shadowTexture;
uniform sampler2D sunShadowTexture;
uniform sampler2D sunInnerShadowTexture;

out vec4 out_frag_color;

vec2 screenpos(){
	return gl_FragCoord.xy/in_rendersize;
}

float shadowSample(vec3 coord){
	float distanceFromLight = texture(shadowTexture ,coord.st).a + 0.005;	
	return distanceFromLight < coord.z ? 0.0 : 1.0 ;
}

float sunShadowSample(vec3 coord){
	float distanceFromLight = texture(sunShadowTexture ,coord.st).z + 0.005;	
	return distanceFromLight < coord.z ? 0.0 : 1.0 ;
}

float sunInnerShadowSample(vec3 coord){
	float distanceFromLight = texture(sunInnerShadowTexture ,coord.st).z + 0.001;	
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
			float radius = 0.002;
			
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
			
			shadowCoordinateWdivide.xy += rnd*vec2(radius/float(in_no_lights),radius)*0.7;
			
			if(shadow_quality > 0.1){
				float x,y;
				for (y = -shadow_quality ; y <=shadow_quality ; y+=1.0)
					for (x = -shadow_quality ; x <=shadow_quality ; x+=1.0)
						shadow += shadowSample(shadowCoordinateWdivide.xyz+vec3(x*radius/float(in_no_lights),y*radius,0));
							
				shadow /= pow(shadow_quality*2+1,2);
			} else {
				shadow = shadowSample(shadowCoordinateWdivide.xyz);
			}

			light_final += shadow*light_angular*light_diffuse*light_distance*brightness*lightStructs[number].color;

		}
	
	return light_final;
}

vec3 getSunLight(vec3 N, vec2 rnd){
	vec4 shadowCoordinateWdivide = v_inner_sun_map_pos / v_inner_sun_map_pos.w;
	float shadow = 1;

	float sunShadowQuality = 1;
	
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
		shadow = 0;

		shadowCoordinateWdivide.xy += rnd*vec2(1,1)*sunradius*0.7;
		
		if(sunShadowQuality > 0.1){
			float x,y;
			for (y = -sunShadowQuality ; y <=sunShadowQuality ; y+=1.0)
				for (x = -sunShadowQuality ; x <=sunShadowQuality ; x+=1.0)
					shadow += sunInnerShadowSample(shadowCoordinateWdivide.xyz+vec3(x*sunradius,y*sunradius,0));
								
			shadow /= pow(sunShadowQuality*2+1,2);
		} else {
			shadow = sunInnerShadowSample(shadowCoordinateWdivide.xyz);
		}

		return sunLightStruct.color * shadow * light_diffuse;
	} 
	else
	{
		sunShadowQuality -= 0.5;
	
		shadowCoordinateWdivide = v_sun_map_pos / v_sun_map_pos.w;
		
		if(0 < shadowCoordinateWdivide.x 
		&& 1 > shadowCoordinateWdivide.x 
		&& 0 < shadowCoordinateWdivide.y 
		&& 1 > shadowCoordinateWdivide.y 
		&& shadowCoordinateWdivide.z > 0
		&& shadowCoordinateWdivide.z < 1)
		{		
			shadow = 0;

			shadowCoordinateWdivide.xy += rnd*vec2(1,1)*sunradius*0.7;
			
			if(sunShadowQuality > 0.1){
				float x,y;
				for (y = -sunShadowQuality ; y <=sunShadowQuality ; y+=1.0)
					for (x = -sunShadowQuality ; x <=sunShadowQuality ; x+=1.0)
						shadow += sunShadowSample(shadowCoordinateWdivide.xyz+vec3(x*sunradius,y*sunradius,0));
									
				shadow /= pow(sunShadowQuality*2+1,2);
			} else {
				shadow = sunShadowSample(shadowCoordinateWdivide.xyz);
			}
			
			//return sunLightStruct.color*sunShadowSample(shadowCoordinateWdivide.xyz);
			
		}

		return sunLightStruct.color * shadow * light_diffuse;
	}
}

void main(void)
{
	float depth = gl_FragCoord.z;
	vec2 screenposition = screenpos();

	vec4 NTexValue = texture(normalTexture, v_texture) * 2.0 - 1.0;
	vec3 N = v_normal;
	
	vec2 rnd = texture(noiseTexture,gl_FragCoord.xy/128).xy * 2 -1;

	vec3 all_lights = vec3(0,0,0);
	
	all_lights += getSunLight(N,rnd);
	
	for(int i = 0; i < no_lights; i++)
	{
		if(i < in_no_lights)
			all_lights += getlight(N,i,rnd);
	}
	
	vec4 fade = texture(baseTexture, v_texture);
	vec4 color = vec4(0.5,0.7,1.0,1.0);
	vec4 reflection = texture(reflectionTexture, screenpos());
	out_frag_color = vec4(in_lightambient + all_lights, 1.0) + reflection * 0.2;
	out_frag_color *= fade;
	out_frag_color[3] = 1;
	//out_frag_color = vec4(0.5,0.7,1.0,1.0)*light_angular;
	//out_frag_color = reflection;
}