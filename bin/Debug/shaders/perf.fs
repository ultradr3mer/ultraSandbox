#version 130
precision mediump float;

//out vec4 out_frag_color;
uniform vec4 in_perfcolor;

void main() 
{
	gl_FragColor = in_perfcolor;
}