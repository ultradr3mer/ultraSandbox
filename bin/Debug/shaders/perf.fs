#version 130
precision mediump float;

out vec4 out_frag_color;
uniform vec4 in_perfcolor;

void main() 
{
	out_frag_color = in_perfcolor;
}