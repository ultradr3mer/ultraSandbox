#version 130
precision lowp float;

uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform sampler2D Texture3;
uniform sampler2D Texture4;
uniform sampler2D Texture5;
in vec2 v_texture;

out vec4 out_frag_color;
uniform vec2 in_screensize;
uniform vec2 in_rendersize;

uniform mat4 invMVPMatrix;

uniform vec3 defDirection;
uniform vec3 defColor;
uniform mat4 defMatrix;
uniform mat4 defInnerMatrix;

uniform float defRadius = 0.0015;
uniform vec3 in_lightambient;

uniform vec3 in_eyepos;

float PI = 3.14159265;
int samples = 5; //samples on the first ring (5-10)
int rings = 4; //ring count (2-6)

vec2 screenpos()
{
	return gl_FragCoord.xy/in_rendersize;
}

float sunShadowSample(vec3 coord, float bias){
	float distanceFromLight = texture(Texture2 ,coord.st).z + bias;	
	return distanceFromLight < coord.z ? 0.0 : 1.0 ;
}

float shadowSampling(sampler2D texture, float quality, vec3 coords, vec2 size,float bias)
{
	float shadow = 0;
	float x,y;
	for (y = -quality ; y <=quality ; y+=1.0)
		for (x = -quality ; x <=quality ; x+=1.0)
		{
			float distanceFromLight = texture(texture ,coords.xy + vec2(x*size.x,y*size.y)).z + bias;	
			shadow += distanceFromLight < coords.z ? 0.0 : 1.0 ;
		}
						
	shadow /= pow(quality*2+1,2);
		
	return shadow;
}

float calcShadow(vec4 g_pos,vec2 rnd, float bias)
{
	float sunShadowQuality = 1;
	
	vec4 v_sun_map_pos = defInnerMatrix * g_pos;
	vec4 shadowCoordinateWdivide = v_sun_map_pos / v_sun_map_pos.w;

	if(0 < shadowCoordinateWdivide.x 
	&& 1 > shadowCoordinateWdivide.x 
	&& 0 < shadowCoordinateWdivide.y 
	&& 1 > shadowCoordinateWdivide.y 
	&& 0 < shadowCoordinateWdivide.z
	&& 1 > shadowCoordinateWdivide.z)
	{		
		shadowCoordinateWdivide.xy += rnd*vec2(1,1)*defRadius*0.5;
		
		return shadowSampling(Texture3,sunShadowQuality,shadowCoordinateWdivide.xyz,vec2(1,1)*defRadius,bias);
	} 
	
	v_sun_map_pos = defMatrix * g_pos;
	shadowCoordinateWdivide = v_sun_map_pos / v_sun_map_pos.w;

	if(0 < shadowCoordinateWdivide.x 
	&& 1 > shadowCoordinateWdivide.x 
	&& 0 < shadowCoordinateWdivide.y 
	&& 1 > shadowCoordinateWdivide.y 
	&& 0 < shadowCoordinateWdivide.z
	&& 1 > shadowCoordinateWdivide.z)
	{		
		shadowCoordinateWdivide.xy += rnd*vec2(1,1)*defRadius*0.5;
		
		return shadowSampling(Texture2,sunShadowQuality-0.5,shadowCoordinateWdivide.xyz,vec2(1,1)*defRadius,bias*2);
	} 
	
	return 1.0;
}

void main() 
{
	float ratio = in_screensize.x/in_screensize.y;
	vec2 screenpos = screenpos();

	vec4 info = texture(Texture5,screenpos);
	
	if(info.a == 1)
		discard;
	
	vec2 rnd = texture(Texture4,gl_FragCoord.xy/128).xy * 2 -1;
	
	vec3 N = texture(Texture1,screenpos).rgb;
	N = N * 2 -1;

	vec4 g_pos = vec4((screenpos * 2 -1),info.a,1);
	g_pos = invMVPMatrix * g_pos;
	g_pos /= g_pos.w;
	
	float angle = clamp(dot(-defDirection, N),0,1);
	
	vec3 viewDir = normalize(g_pos.xyz-in_eyepos);
	
	vec3 refn  = reflect(viewDir,N);
	float spec = clamp(dot(-defDirection, refn),0,1);
	spec = info.r*info.r*10.0*pow(spec,info.g*info.g*100.0);
	
	float bias = 0.001/clamp(angle,0.01,1.0);
	float shadow = calcShadow(g_pos, rnd, bias);
	
	out_frag_color.rgb = in_lightambient+defColor*angle*shadow;
	out_frag_color.a = spec*shadow*angle;
}