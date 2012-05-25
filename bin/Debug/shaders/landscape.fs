#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;

uniform float in_waterlevel;
uniform int in_pass;

in vec4 g_pos;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;
in vec3 light;

uniform sampler2D Texture1;
uniform sampler2D Texture2;

float fade_angle = 0.55;
float fade_angle_b = 0.50;
float fade_width = fade_angle_b - fade_angle;

in vec3 v_eyedirection;

uniform sampler2D EnvTexture1;
uniform sampler2D EnvTexture2;
uniform sampler2D EnvTexture3;
uniform sampler2D EnvTexture4;
uniform sampler2D EnvTexture5;
uniform sampler2D EnvTexture6;

out vec4 out_frag_color;

vec4 get_env(vec3 refn){
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

vec4 surface_color(){
	vec4 NTexValue = texture(Texture1, v_texture) * 2.0 - 1.0;
	vec3 N = normalize(NTexValue[2] * v_normal + NTexValue[0] * v_tangent + NTexValue[1] * v_bnormal);
	
	vec3 refn = reflect(v_eyedirection,N);
	
	vec4 env = vec4(0,0,0,0);//get_env(refn);
	
	float diffuse = clamp(dot(light, N), 0.0, 1.0);

		//out_frag_color = vec4(N,1);
  
  		vec4 color = vec4(0.5,0.7,1.0,1.0);
		
		float fade = 1;
			
		if (N[1] < fade_angle) {
			if (N[1] > fade_angle_b) {
				fade = (fade_angle_b-N[1])/fade_width;
				color = color*fade+texture(Texture2, v_texture*8.0)*(1.0-fade);
			} else {
				fade = 0;
				color = texture(Texture2, v_texture*8.0);
			}
		}
		
		return vec4(in_lightambient + diffuse * in_lightcolor, 1.0)*color+env*0.4*(1.0-fade);
}

void main(void)
{
	if (in_pass == 0.0) {
		if( g_pos[1]-in_waterlevel < 0.0){
			vec4 surface_color = surface_color();
			float visibility = 1.0+(g_pos[1]-in_waterlevel)/in_waterlevel/0.7;
			
			surface_color[3] = visibility;

			out_frag_color = surface_color;
		} else {
			out_frag_color = surface_color();
		}
	}
	if (in_pass == 1.0) {
		if( g_pos[1]-in_waterlevel < 0.0 ){
			discard;
		} else {
			out_frag_color = surface_color();
		}
	}
}