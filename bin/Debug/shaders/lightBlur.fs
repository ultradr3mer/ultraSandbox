#version 130
precision mediump float;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
in vec2 v_texture;


out vec4 out_frag_color;
uniform vec2 in_rendersize;


float PI = 3.14159265;
float samples = 1; //samples on the first ring (5-10)
int rings = 4; //ring count (2-6)

vec4 readSample(vec2 coo)
{
	vec4 sample = texture(Texture1, coo);
	vec3 normal = sample.xyz * 2 - 1;
	return vec4(normal,sample.a);	
}

void main() {
	vec4 curpixel = readSample(v_texture);
	
	vec2 rastersize = 1/in_rendersize;

	vec4 base_color = vec4(0.0,0.0,0.0,0.0);
	
	if(curpixel.a == 1)
		discard;

	vec4 col;	
	float s = 0;
	vec4 sample;
	float normdiff;
	vec2 coords;
	
	float x,y;
	
	for (x = -samples ; x <= samples; ++x)
	{
		for (y = -samples ; y <= samples; ++y)
		{
			coords = vec2(v_texture.s+rastersize.x*x,v_texture.t+rastersize.y*y);

			sample = readSample( coords );
				
			normdiff = clamp(dot(sample.xyz,curpixel.xyz)-0.9, 0.0, 1);

			col += texture(Texture2,coords) * normdiff;
			s += normdiff;
		}
	}
		
	out_frag_color = col/s;
}