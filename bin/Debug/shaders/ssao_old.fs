#version 130
precision mediump float;
uniform sampler2D Texture1;
uniform vec2 in_vector;
in vec2 vTextureCoord;

out vec4 out_frag_color;


float PI = 3.14159265;
int samples = 10; //samples on the first ring (5-10)
int rings = samples; //ring count (2-6)
float near = 1.0; //Z-near
float far = 100.0; //Z-far

vec2 rand(in vec2 coord) //generating random noise
{
	float noiseX = (fract(sin(dot(coord ,vec2(12.9898,78.233))) * 43758.5453));
	float noiseY = (fract(sin(dot(coord ,vec2(12.9898,78.233)*2.0)) * 43758.5453));
	return vec2(noiseX,noiseY);
}

float readDepth(in vec2 coord) 
{
	return (2.0 * near) / (far + near - texture(Texture1, coord ).x * (far-near)); 	
}

float compareDepths( in float depth1, in float depth2 ) {
	float aorange = 200;
	float diff = (depth1-depth2)*aorange;
	float ao = (diff/2)/((diff/2)*(diff/2)+0.25)/aorange;
	return ao;
}

void main() {
	vec2 rand = rand(vTextureCoord);
	
	float depth = readDepth(vTextureCoord);
	float d;
	
	float pw;
	float ph;
	
	float ao;	
	float s = 0;
	float aoscale = 0.02;
	
	for (int i = 1 ; i < rings; ++i)
	{
		for (int j = 0 ; j < samples; ++j)
		{	
			float step = PI*2.0 / float(samples);
			pw = cos(float(j)*step)*i;
			ph = sin(float(j)*step)*i;
			d = readDepth( vec2(vTextureCoord.s+pw*in_vector.s,vTextureCoord.t+ph*in_vector.t) );
			ao += compareDepths(depth,d)/aoscale;	
			s ++;
		}
	}
	
	ao /= s;
	ao *= 6;
	ao += 0.1;
	ao = 1-ao;
	ao = min(ao,1);
	
	out_frag_color = vec4(1,1,1,1)*ao;
}