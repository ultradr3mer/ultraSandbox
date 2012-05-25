#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;
uniform vec3 in_lightdir;

const int no_lights = 6;

uniform int in_no_lights;

uniform vec3 in_n_light[no_lights];
uniform vec3 in_n_lightdir[no_lights];
uniform vec3 in_n_lightcolor[no_lights];

uniform int curLight;
uniform vec2 in_rendersize;

in vec4 g_pos;
in vec3 v_eyedirection;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;

uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform sampler2D Texture3;

uniform sampler2D EnvTexture1;
uniform sampler2D EnvTexture2;
uniform sampler2D EnvTexture3;
uniform sampler2D EnvTexture4;
uniform sampler2D EnvTexture5;
uniform sampler2D EnvTexture6;

out vec4 out_frag_color;

vec2 screenpos(){
	return gl_FragCoord.xy/in_rendersize;
}

void main(void)
{
	vec2 screenpos = screenpos();
	
	float leftclip = curLight/float(in_no_lights);
	float rightclip = (curLight+1)/float(in_no_lights);
	
	if(screenpos.x > rightclip || screenpos.x < leftclip){
		discard;
	}else{
		out_frag_color = vec4(1,1,1,gl_FragCoord.z);
	}
}