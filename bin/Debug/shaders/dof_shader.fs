#version 130
precision mediump float;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
in vec2 v_texture;


out vec4 out_frag_color;
uniform vec2 in_screensize;


float PI = 3.14159265;
int samples = 4; //samples on the first ring (5-10)
int rings = 4; //ring count (2-6)

vec4 sample(vec2 coo)
{
	vec4 color = texture(Texture1, coo);
	return color;	
}

void main() {
	int i;
	vec4 base_color = vec4(0.0,0.0,0.0,0.0);
	
	vec2 rnd = texture(Texture2,gl_FragCoord.xy/128).xy * 2 -1;
	float step = PI*2.0 / float(samples);
	
	vec4 curPixel = sample(v_texture);

	float pw;
	float ph;
	float ringcoord;
	
	vec2 target_coord;
	vec2 size = 10/in_screensize/rings*curPixel.a;
	
	vec4 col;	
	float s = 0;
	float aoscale = 0.02;
	
	for (int i = 1 ; i <= rings; ++i)
	{
		for (int j = 0 ; j < samples; ++j)
		{	
			ringcoord = float(j)*step+rnd.x;
			pw = cos(ringcoord)*i;
			ph = sin(ringcoord)*i;
			target_coord = vec2(v_texture.s+pw*size.s,v_texture.t+ph*size.t);
			col += texture(Texture1, target_coord);
		}
	}
	
	out_frag_color = col/samples/rings;
	//out_frag_color = curPixel;
}