#version 130
precision mediump float;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform vec2 in_vector;
in vec2 v_texture;


out vec4 out_frag_color;


float PI = 3.14159265;
int samples = 20; //samples on the first ring (5-10)
int rings = samples; //ring count (2-6)

vec4 sample(vec2 coo, float perc)
{
	vec4 color = texture(Texture1, coo);
	//color += (1-perc)*vec4(1.0,0.0,1.0,1.0);
	return color;	
}

vec2 scale(vec2 coo, float factor)
{
	return (coo - vec2(0.5,0.5)) * factor + vec2(0.5,0.5);
}

void main() {
	float mod = texture(Texture2, v_texture)[0]*5;
	float perc = in_vector.s;
	int i;
	vec4 base_color = vec4(0.0,0.0,0.0,0.0);
	float shift;
	vec2 dir_shift;

	float pw;
	float ph;
	
	vec4 col;	
	float s = 0;
	
	float step = PI*2.0 / float(samples);
	
	for (int i = 1 ; i < samples; ++i)
	{
			float rel_i = i / float(samples);
			col += sample( scale(vec2(v_texture.s,1-v_texture.t), 1-(1-perc)*mod*rel_i), perc);
			s ++;
	}
	
	out_frag_color = col/s;
}