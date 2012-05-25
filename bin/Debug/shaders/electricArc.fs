#version 130

precision highp float;

in vec2 v_texture;

uniform vec2 in_rendersize;

uniform sampler2D baseTexture;
uniform sampler2D backDepthTexture;

uniform vec4 in_color;

out vec4 out_frag_color;

vec2 screenpos(){
	return gl_FragCoord.xy/in_rendersize;
}

void main(void)
{
	float depth = gl_FragCoord.z;
	
	vec2 screenposition = screenpos();

	if(depth > texture(backDepthTexture, screenposition).x){
		discard;
	}
	out_frag_color = texture(baseTexture,v_texture)*in_color;
}