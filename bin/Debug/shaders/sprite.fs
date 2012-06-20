#version 130
precision lowp float;
in vec2 v_texture;

uniform vec4 in_color;

uniform sampler2D baseTexture;

out vec4 out_frag_color;


void main() {
	out_frag_color = texture(baseTexture, v_texture)*in_color;
}