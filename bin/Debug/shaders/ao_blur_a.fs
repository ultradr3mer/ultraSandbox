#version 130
precision mediump float;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform vec2 in_rendersize;
in vec2 v_texture;


out vec4 out_frag_color;


float PI = 3.14159265;
float samples = 5.5; //samples on the first ring (5-10)

vec4 readSample(vec2 coo)
{
	vec4 sample = texture(Texture1, coo);
	vec3 normal = normalize(sample.xyz * 2 - 1);
	
	return vec4(normal,sample.a);	
}

void main() {
	vec4 curpixel = readSample(v_texture);
	
	if(curpixel != vec4(1,0,0,0.5)) {
		vec2 rastersize = 1/in_rendersize;

		vec4 base_color = vec4(0.0,0.0,0.0,0.0);

		float col;	
		float s = 0;
		vec4 sample;
		float normdiff;
	
		col += curpixel.a*0.1;
		s = 0.1;
		
		for (float i = -samples ; i <= samples; ++i)
		{
			sample = readSample( vec2(v_texture.s+rastersize.x*i,v_texture.t) );
				
			normdiff = clamp(dot(sample.xyz,curpixel.xyz), 0.0, 1);
			
			normdiff = pow(normdiff,10);
			col += sample.a * normdiff;
			s += normdiff;
		}
		
		for (float i = -samples ; i <= samples; ++i)
		{
			sample = readSample( vec2(v_texture.s,v_texture.t+rastersize.y*i) );
				
			normdiff = clamp(dot(sample.xyz,curpixel.xyz), 0.0, 1);
			
			normdiff = pow(normdiff,10);
			col += sample.a * normdiff;
			s += normdiff;
		}
		
		vec4 oldsample = texture(Texture2, v_texture);
		float ao = col/s * 0.3 + oldsample.a * 0.7;
		out_frag_color = vec4(curpixel.xyz,ao);
		//out_frag_color = curpixel;
	
	//out_frag_color = vec4(1,1,1,1)*curpixel.a;

	} else {
		out_frag_color = vec4(1,0,0,0.5);
	}
}