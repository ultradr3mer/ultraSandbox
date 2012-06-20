#version 130
precision mediump float;

uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform sampler2D Texture3;
uniform sampler2D Texture4;
uniform sampler2D Texture5;
uniform sampler2D Texture6;
uniform sampler2D Texture7;
uniform sampler2D Texture8;
in vec2 v_texture;

uniform vec2 in_rendersize;

out vec4 out_frag_color;
uniform vec2 in_screensize;

uniform mat4 invMVPMatrix;

uniform vec3 in_eyepos;

float PI = 3.14159265;
int samples = 5; //samples on the first ring (5-10)
int rings = 4; //ring count (2-6)

in vec3 v_view;

vec3 get_env(vec3 refn){
	vec3 tmpn = refn;
	
	refn.x = -refn.x;

	tmpn.x *= sign(tmpn.x);
	tmpn.y *= sign(tmpn.y);
	tmpn.z *= sign(tmpn.z);
		
	vec4 env = vec4(0,0,0,1);
	if(tmpn.x > tmpn.y && tmpn.x > tmpn.z){
		if(refn.x < 0){
			vec2 envuv = vec2(refn.z/refn.x,refn.y/refn.x)*0.5+0.5;
			return texture(Texture3,envuv).rgb;
		} else {
			vec2 envuv = vec2(refn.z/refn.x,-refn.y/refn.x)*0.5+0.5;
			return texture(Texture1,envuv).rgb;
		}
	} 
	if(tmpn.z > tmpn.y && tmpn.z > tmpn.x){
		if(refn.z < 0){
			vec2 envuv = vec2(-refn.x/refn.z,refn.y/refn.z)*0.5+0.5;
			return texture(Texture4,envuv).rgb;
		} else {
			vec2 envuv = vec2(-refn.x/refn.z,-refn.y/refn.z)*0.5+0.5;
			return texture(Texture2,envuv).rgb;
		}
	}
	if(tmpn.y > tmpn.z && tmpn.y > tmpn.x){
		if(refn.y < 0){
			vec2 envuv = vec2(-refn.x/refn.y,-refn.z/refn.y)*0.5+0.5;
			return texture(Texture6,envuv).rgb;
		} else {
			vec2 envuv = vec2(refn.x/refn.y,-refn.z/refn.y)*0.5+0.5;
			return texture(Texture5,envuv).rgb;
		}
	} 
}

vec2 screenpos()
{
	return gl_FragCoord.xy/in_rendersize;
}

void main() {
	vec4 info = texture(Texture8, v_texture);
	vec2 screenpos = screenpos();
	
	if(info.a == 1)
		discard;

	vec3 N = texture(Texture7, v_texture).rgb;
	N = N * 2 -1;
	
	vec4 g_pos = vec4((screenpos * 2 -1),info.a,1);
	g_pos = invMVPMatrix * g_pos;
	g_pos /= g_pos.w;
	
	vec3 viewDir = normalize(g_pos.xyz-in_eyepos);
	vec3 refn  = reflect(viewDir,N);
	
	out_frag_color.rgb = get_env(refn)*info.b;
	out_frag_color.a = 1.0;
}