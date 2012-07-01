#version 130
precision mediump float;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform vec2 in_vector;

in vec2 v_texture;

out vec4 out_frag_color;
uniform vec2 in_screensize;


float PI = 3.14159265;
int samples = 4; //samples on the first ring (5-10)
int rings = 4; //ring count (2-6)


void main() {	
	out_frag_color = texture(Texture2,v_texture);
	
	//float focus = texture(Texture1,vec2(0.5,0.5)).a;
	
	float distanceterm = texture(Texture1,v_texture).a-in_vector[0];
	//distanceterm *= in_vector[1]*sign(distanceterm);
	distanceterm *= in_vector[1]*distanceterm;
	
	out_frag_color[3] = distanceterm;
	//out_frag_color = vec4(1,1,1,1)*distanceterm;
}