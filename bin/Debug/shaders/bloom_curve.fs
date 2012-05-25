#version 130
precision mediump float;
uniform sampler2D Texture1;
in vec2 v_texture;


out vec4 out_frag_color;
uniform vec2 in_screensize;

uniform float bloomExp = 2;
uniform float bloomStrength = 2;


float PI = 3.14159265;
int samples = 10; //samples on the first ring (5-10)

void main() {
	vec4 color = texture(Texture1, v_texture);
	out_frag_color = vec4(pow(color.r,bloomExp),pow(color.g,bloomExp),pow(color.b,bloomExp),1)*bloomStrength;
}