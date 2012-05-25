#version 130
precision mediump float;
uniform sampler2D Texture2;
uniform sampler2D Texture3;
uniform vec2 in_vector;

in vec2 v_texture;
uniform float in_near;
uniform float in_far;

out vec4 out_frag_color;
uniform vec2 in_screensize;

uniform float aosize = 1;
uniform float aorange = 400;
uniform float noise = 0.1;

uniform float totStrength = 0.7;
uniform float strength = 0.07;
uniform float falloff = 0.002;
uniform float rad = 0.01;

float PI = 3.14159265;
int samples = 16; //samples on the first ring (5-10)
float near = 1.0; //Z-near
float far = 100.0; //Z-far

vec3 rand(in vec2 coord) //generating random noise
{
	float noiseX = (fract(sin(dot(coord ,vec2(12.9898,78.233))) * 43758.5453)) * 2 -1;
	float noiseY = (fract(sin(dot(coord ,vec2(12.9898,78.233)*2.0)) * 43758.5453)) * 2 -1;
	float noiseZ = (fract(sin(dot(coord ,vec2(12.9898,78.233)*3.0)) * 43758.5453)) * 2 -1;
	return vec3(noiseX,noiseY,noiseZ);
}

vec3 readNormal(in vec2 coord) 
{
	return normalize(vec3(texture(Texture2,coord)) * 2 - 1);
}

vec4 readSample(in vec2 coord)
{
	vec4 sample = texture(Texture2,coord);
	vec3 normal = normalize(sample.xyz * 2 - 1);
	float depth = (1-sample.a);//*(in_far-in_near);
	return vec4(normal,depth);
}

float compareDepths( in float depth1, in float depth2 ) {
	float diff = (depth1-depth2)*aorange;
	float ao = diff/(diff*diff+0.25);
	return ao;
}

void main() {
	// these are the random vectors inside a unit sphere
	vec3 pSphere[16] = vec3[](vec3(0.53812504, 0.18565957, -0.43192),vec3(0.13790712, 0.24864247, 0.44301823),vec3(0.33715037, 0.56794053, -0.005789503),vec3(-0.6999805, -0.04511441, -0.0019965635),vec3(0.06896307, -0.15983082, -0.85477847),vec3(0.056099437, 0.006954967, -0.1843352),vec3(-0.014653638, 0.14027752, 0.0762037),vec3(0.010019933, -0.1924225, -0.034443386),vec3(-0.35775623, -0.5301969, -0.43581226),vec3(-0.3169221, 0.106360726, 0.015860917),vec3(0.010350345, -0.58698344, 0.0046293875),vec3(-0.08972908, -0.49408212, 0.3287904),vec3(0.7119986, -0.0154690035, -0.09183723),vec3(-0.053382345, 0.059675813, -0.5411899),vec3(0.035267662, -0.063188605, 0.54602677),vec3(-0.47761092, 0.2847911, -0.0271716));
   
	// grab a normal for reflecting the sample rays later on
	vec3 fres = normalize(vec3(texture(Texture3,gl_FragCoord.xy/128)) * 2 - 1);
	//vec3 fres = rand(v_texture);
	
	vec4 curpixel = readSample(v_texture);
	
	//vec3 tangent = normalize(cross(normal,vec3(1,0,0)));
	//vec3 binormal = normalize(cross(tangent, normal));
			
	//depth = texture(Texture1,v_texture).x;
	vec2 ratio = vec2(in_screensize.y/in_screensize.x,1);
	float radD = rad/curpixel.a;
	
	vec2 scalefactor = ratio* radD;
	
	vec2 samplepos, se;
	vec3 occnorm, ray, randvec;
	float normdiff, occdepth, depthdifference;
    float bl = 0.0;
	float mask = 0.0;
	
	for (int i = 0 ; i < samples; ++i)
	{	
		//random vector 
		//ray = rand(v_texture*i);
		//ray = rand(v_texture*i);
		ray = reflect(pSphere[i],fres);
		
		// if the ray is outside the hemisphere then change direction
		//ray = sign(dot(ray,curpixel.xyz) )*ray;
		
		se = v_texture + ray.xy*scalefactor;
								
		// get the depth of the occluder fragment
		vec4 sample = readSample(se);
				
		// if depthDifference is negative = occluder is behind current fragment
		depthdifference = curpixel.a-sample.a;
 
		// calculate the difference between the normals as a weight
		normdiff = (1.0-dot(sample.xyz,curpixel.xyz));
		
		// the falloff equation, starts at falloff and is kind of 1/x^2 falling
		bl += step(falloff,depthdifference)*normdiff*(1.0-smoothstep(falloff,strength,depthdifference));
		//bl += compareDepths(curpixel.a,sample.a);
		//mask += normdiff;
	}
	
	// output the result
	float ao = 0.5-totStrength*bl/samples;
   
	out_frag_color = vec4(curpixel.xyz*2+1,ao);
	//out_frag_color = vec4(tangent*0.5+0.5,1);
	//out_frag_color = vec4(1,1,1,1)*mask*bl;
	//out_frag_color = vec4(curpixel.xyz,1);
}