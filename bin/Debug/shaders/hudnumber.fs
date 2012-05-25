#version 130
precision mediump float;
uniform sampler2D baseTexture;
uniform sampler2D base2Texture;
uniform sampler2D base3Texture;
in vec2 v_texture;

out vec4 out_frag_color;
uniform vec2 in_screensize;
uniform vec4 in_hudcolor;
uniform float in_hudvalue;

float PI = 3.14159265;

void main() {
	float y_chord = asin((v_texture.y-0.5)*2.0);
	y_chord = y_chord*0.1+0.05+in_hudvalue*0.1;
	out_frag_color = (texture(baseTexture, vec2(v_texture.x,y_chord))+texture(base2Texture, v_texture))*texture(base3Texture, v_texture)*in_hudcolor;
}