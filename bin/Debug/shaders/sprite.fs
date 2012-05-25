#version 130
precision lowp float;
in vec2 v_texture;
in vec4 g_pos;

uniform vec2 in_rendersize;
uniform vec4 in_color;

uniform sampler2D baseTexture;
uniform sampler2D backDepthTexture;

out vec4 out_frag_color;

vec2 screenpos(){
	return gl_FragCoord.xy/in_rendersize;
}

void main() {
	float depth = gl_FragCoord.z;
	
	vec2 screenposition = screenpos();
	
	out_frag_color = texture(baseTexture, v_texture)*in_color;
}