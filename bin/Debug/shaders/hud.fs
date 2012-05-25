#version 130
precision mediump float;
uniform sampler2D baseTexture;
in vec2 v_texture;

out vec4 out_frag_color;
uniform vec2 in_screensize;
uniform vec4 in_hudcolor;

void main() {
	out_frag_color = texture(baseTexture, v_texture)*in_hudcolor;
}