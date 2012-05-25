#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;
uniform vec3 in_lightdir;

in vec4 g_pos;
in vec3 v_eyedirection;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;
in vec3 light;

uniform sampler2D Texture1;
uniform sampler2D Texture2;

uniform sampler2D EnvTexture1;
uniform sampler2D EnvTexture2;
uniform sampler2D EnvTexture3;
uniform sampler2D EnvTexture4;
uniform sampler2D EnvTexture5;
uniform sampler2D EnvTexture6;

out vec4 out_frag_color;

vec4 get_env(vec3 refn){
	float env_strenth = 0.2;

	vec3 tmpn = refn;

	if(tmpn.x < 0)
		tmpn.x *= -1;
	if(tmpn.y < 0)
		tmpn.y *= -1;
	if(tmpn.z < 0)
		tmpn.z *= -1;
		
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

float getlight(vec3 N){
	float brightness = 2;

	float ang_hardness = 10;
	float ang_limit = 0.7;
	float lenth_mult = 0.2;
		
	vec3 light_vec = normalize(light-g_pos.xyz);
	vec3 light_dir = normalize(in_lightdir);
	
	float light_angular = clamp((dot(light_vec,-light_dir)-ang_limit)*ang_hardness, 0.0, 1.0);
	float light_diffuse = clamp(dot(light_vec, N), 0.0, 1.0);
	float light_distance = 1/exp2(length(light-g_pos.xyz)*lenth_mult);
	
	return light_angular*light_diffuse*light_distance*brightness;
}

float getspec(vec3 refn,float light_final){
	
	float specular_strenth = 0.6;
	float specular_exponent = 10;
	
	vec3 ref_vector = normalize(light-g_pos.xyz);
	vec3 eye_dir = refn;
	
	float specular = clamp(dot(ref_vector, eye_dir), 0.0, 1.0);
	
	float final_spac = pow(specular,specular_exponent)*specular_strenth;
	
	return final_spac*light_final;
	
}

void main(void)
{
	vec4 NTexValue = texture(Texture1, v_texture) * 2.0 - 1.0;
	vec3 N = normalize(NTexValue[2] * v_normal + NTexValue[0] * v_tangent + NTexValue[1] * v_bnormal);
	
	vec3 refn = reflect(v_eyedirection,N);
	
	float light_final = getlight(N);
	float specular_final = getspec(refn,light_final);

	vec4 env = get_env(refn);
	
  //vec4 Color = texture(Texture1, v_texture)*vec4(0.8,0.5,0.8,1.0);
  vec4 Color = texture(Texture2, v_texture)*vec4(0.5,0.7,1.0,1.0);//*vec4(0.8,0.5,0.8,1.0);;
  float diffuse = clamp(dot(light, N), 0.0, 1.0);
  out_frag_color = vec4(in_lightambient + light_final * in_lightcolor, 1.0)*Color+specular_final+env;
  //out_frag_color = env;
  //out_frag_color = vec4(specular_final,specular_final,specular_final,1);
  //out_frag_color = texture(EnvTexture1,vec2(refn.x,refn.y));
  //out_frag_color[3] = 1;
}