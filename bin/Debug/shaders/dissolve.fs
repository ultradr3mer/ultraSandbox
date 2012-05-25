#version 130

precision highp float;

in vec2 v_texture;

uniform vec2 in_rendersize;
uniform vec4 in_color;

uniform sampler2D baseTexture;

out vec4 out_frag_color;

uniform float in_time;
uniform float in_mod;

vec2 screenpos(){
	return gl_FragCoord.xy/in_rendersize;
}

void main(void)
{
	vec2 noiseOffset = vec2(0,1)*in_time;
	
	if (texture(baseTexture,v_texture).r < in_mod)
		out_frag_color = in_color;
	else
		out_frag_color = vec4(0,0,0,0);
}