#version 130
precision mediump float;
in vec2 v_texture;

uniform vec2 in_rendersize;
uniform vec4 in_color;

uniform sampler2D baseTexture;

out vec4 out_frag_color;

vec2 screenpos(){
	return gl_FragCoord.xy/in_rendersize;
}

void main() {
	float depth = gl_FragCoord.z;
	
	vec2 screenposition = screenpos();
	
	out_frag_color = texture(baseTexture, v_texture)*in_color;
}