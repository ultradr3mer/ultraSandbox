#version 130

precision highp float;

const vec3 ambient = vec3(0.1, 0.1, 0.1);
const vec3 lightVecNormalized = normalize(vec3(0.5, 0.5, 2.0));
const vec3 lightColor = vec3(0.9, 0.9, 0.7);
uniform vec2 in_screensize;

in vec3 normal;
in vec2 v_texture;
uniform sampler2D Texture1;
uniform sampler2D Texture2;

uniform float in_time;

out vec4 out_frag_color;

vec2 screenpos(){
	return gl_FragCoord.xy/in_screensize;
}

void main(void)
{
	vec2 screenpos = screenpos();
	vec2 waterpos = v_texture*64.0+vec2(0.05,0.05)*in_time;
		
	vec4 refracttex = texture(Texture2, waterpos);
	vec2 refractvec = vec2(refracttex[0]*0.02-0.01,-0.003);
		
	vec4 Color = texture(Texture1, screenpos+refractvec);
	float diffuse = clamp(dot(lightVecNormalized, normalize(normal)), 0.0, 1.0);
	out_frag_color = Color;
	out_frag_color[3] = 0.3;
	
	//out_frag_color = texture(Texture2, waterpos);
}