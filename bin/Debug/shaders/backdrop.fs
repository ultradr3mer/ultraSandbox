#version 130
precision mediump float;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
in vec2 v_texture;


out vec4 out_frag_color;
uniform vec2 in_screensize;


float PI = 3.14159265;
int samples = 5; //samples on the first ring (5-10)
int rings = 4; //ring count (2-6)

void main() {
	out_frag_color = texture(Texture1, v_texture);
	out_frag_color.a = 1;
}