#version 130
precision mediump float;
uniform sampler2D Texture1;
in vec2 v_texture;


out vec4 out_frag_color;
uniform vec2 in_screensize;
uniform vec2 in_vector;


float PI = 3.14159265;


vec4 sample(vec2 coo)
{
	vec4 color = texture(Texture1, coo);
	return color;	
}

void main() {
	int i;
	vec4 base_color = vec4(0.0,0.0,0.0,0.0);
	
	int samples = 8; //samples on the first ring (5-10)

	float step = PI*2.0 / float(samples);

	float pw;
	float ph;
	float ringcoord;
	
	vec2 size = in_vector/in_screensize;
	
	vec4 col;	
	float s = 0;
	float aoscale = 0.02;
	
	int totalsamples = 0;
	vec2 targetpos;
	
	float stepsize = 2.0 / samples;
	
	for	(float i = -1 ; i <= 1; i += stepsize)
	{
		targetpos = v_texture + vec2(i,0) * size;
		
		col += sample( targetpos );
		totalsamples++;
	}
	
	for	(float i = -1 ; i <= 1; i += stepsize)
	{
		targetpos = v_texture + vec2(0,i) * size;
		
		col += sample( targetpos );
		totalsamples++;
	}
		
	out_frag_color = col/totalsamples;
}