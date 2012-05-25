#version 130
precision mediump float;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform sampler2D Texture3;
uniform sampler2D Texture4;
uniform vec2 in_rendersize;
in vec2 v_texture;


out vec4 out_frag_color;


float PI = 3.14159265;
int samples = 3; //samples on the first ring (5-10)
int rings = 2; //ring count (2-6)

vec4 readSample(vec2 coo)
{
	vec4 sample = texture(Texture1, coo);
	vec3 normal = normalize(sample.xyz * 2 - 1);
	
	return vec4(normal,sample.a);	
}

void main() {
	vec4 curpixel = readSample(v_texture);
	//curpixel.rgb = normalize(texture(Texture4,v_texture).rgb * 2 - 1);
	
	if(curpixel != vec4(1,0,0,0.5)) {
		
		int i;
		vec4 base_color = vec4(0.0,0.0,0.0,0.0);
		
		vec2 rnd = texture(Texture2,gl_FragCoord.xy/128).xy * 2 -1;

		vec2 size = 7/in_rendersize/rings;

		float pw;
		float ph;
		
		float col;	
		float s = 0;
		vec4 sample;
		float normdiff;
		
		float ringcoord;
	
		col += curpixel.a*0.1;
		s = 0.1;
	
		float step = PI*2.0 / float(samples);
		for (int i = 1 ; i < rings; ++i)
		{
			for (int j = 0 ; j < samples; ++j)
			{
				ringcoord = float(j)*step+rnd.x;
				pw = cos(ringcoord)*i;
				ph = sin(ringcoord)*i;
				
				sample = readSample( vec2(v_texture.s+pw*size.s,v_texture.t+ph*size.t) );
				
				normdiff = clamp(dot(sample.xyz,curpixel.xyz), 0.0, 1.0);
				if(normdiff > 0.7){
					
					//normdiff = pow(normdiff,10);
					//col += sample.a * normdiff;
					//s += normdiff;
					col += sample.a;
					s ++;
				}
			}
		}
		
		vec4 oldsample = texture(Texture3, v_texture);
		float ao = col/s * 0.3 + oldsample.a * 0.7;
		
		out_frag_color = vec4(curpixel.xyz,ao);
	//out_frag_color = vec4(1,1,1,1)*curpixel.a;

	} else {
		out_frag_color = vec4(1,0,0,0.5);
	}
	//out_frag_color = curpixel;
}