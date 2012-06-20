#version 130

precision highp float;

in vec2 v_texture;

uniform vec2 in_rendersize;
uniform vec4 in_color;

uniform sampler2D baseTexture;
uniform sampler2D base2Texture;

out vec4 out_frag_color;

uniform float in_time;

vec2 screenpos(){
	return gl_FragCoord.xy/in_rendersize;
}

void main(void)
{
	float depth = gl_FragCoord.z;
	
	vec2 screenposition = screenpos();
	
	vec2 noiseOffset = vec2(0,1)*in_time;
	
	out_frag_color = texture(baseTexture,v_texture)*texture(base2Texture,v_texture*vec2(1,0.5)+noiseOffset)*in_color;
}