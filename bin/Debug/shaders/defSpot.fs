#version 130
precision lowp float;

uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform sampler2D Texture3;
uniform sampler2D Texture4;
in vec2 v_texture;

out vec4 out_frag_color;
uniform vec2 in_screensize;
uniform vec2 in_rendersize;

uniform int in_no_lights;
uniform int curLight;

uniform mat4 invMVPMatrix;

uniform vec3 defDirection;
uniform vec3 defColor;
uniform vec3 defPosition;
uniform mat4 defMatrix;
uniform mat4 defInnerMatrix;
uniform mat4 invMMatrix;

uniform vec3 in_eyepos;

uniform float defRadius = 0.004;

uniform vec3 in_lightambient;

float PI = 3.14159265;
int samples = 5; //samples on the first ring (5-10)
int rings = 4; //ring count (2-6)

vec2 screenpos()
{
	return gl_FragCoord.xy/in_rendersize;
}

float shadowSampling(sampler2D texture, float quality, vec3 coords, vec2 size,float bias)
{
	float shadow = 0;
	float x,y;
	for (y = -quality ; y <=quality ; y+=1.0)
		for (x = -quality ; x <=quality ; x+=1.0)
		{
			float distanceFromLight = texture(texture ,coords.xy + vec2(x*size.x,y*size.y)).a + bias;	
			shadow += distanceFromLight < coords.z ? 0.0 : 1.0 ;
		}
						
	shadow /= pow(quality*2+1,2);
		
	return shadow;
}

float calcShadow(vec4 g_pos,vec2 rnd, float bias)
{
	float shadowQuality = 1;
	
	vec4 v_sun_map_pos = defMatrix * g_pos;
	vec4 shadowCoordinateWdivide = v_sun_map_pos / v_sun_map_pos.w;

	if(0 < shadowCoordinateWdivide.x 
	&& 1 > shadowCoordinateWdivide.x 
	&& 0 < shadowCoordinateWdivide.y 
	&& 1 > shadowCoordinateWdivide.y 
	&& 0 < shadowCoordinateWdivide.z
	&& 1 > shadowCoordinateWdivide.z)
	{		
		shadowCoordinateWdivide.xy += rnd*vec2(1,1)*defRadius;
		
		float light_angular;
		light_angular = clamp((1-length(shadowCoordinateWdivide.xy*2.0-1.0))*10.0,0.0,1.0);
		
		shadowCoordinateWdivide.x = (shadowCoordinateWdivide.x + curLight) / float(in_no_lights);
		
		float light_distance = pow(1-shadowCoordinateWdivide.z,1.3);

		return 10.0*light_angular*light_distance*shadowSampling(Texture2,shadowQuality,shadowCoordinateWdivide.xyz,vec2(1/float(in_no_lights),1)*defRadius,bias);
		//return texture(Texture2,shadowCoordinateWdivide.xy).x;
	} 
	
	return 0.0;
}

void main() 
{
	float ratio = in_screensize.x/in_screensize.y;
	vec2 screenpos = screenpos();
	
	vec4 info = texture(Texture4,screenpos);
	
	if(info.a == 1)
		discard;
	
	vec2 rnd = texture(Texture3,gl_FragCoord.xy/128).xy * 2 -1;
	
	vec3 N = texture(Texture1,screenpos).rgb;
	N = N * 2 -1;
		
	vec4 g_pos = vec4((screenpos * 2 -1),info.a,1);
	g_pos = invMVPMatrix * g_pos;
	g_pos /= g_pos.w;
	
	vec3 lightDirection = normalize(defPosition - g_pos.xyz);

	float angle = clamp(dot(lightDirection, N),0,1);
	
	vec3 viewDir = normalize(g_pos.xyz-in_eyepos);
	vec3 refn  = reflect(viewDir,N);
	float spec = clamp(dot(lightDirection, refn),0,1);
	spec = info.r*info.r*10.0*pow(spec,info.g*info.g*100.0);
	
	float bias = 0.008/clamp(angle,0.01,1.0);
	float shadow = calcShadow(g_pos, rnd, bias);

	out_frag_color.rgb = defColor*angle*shadow;
	out_frag_color.a = spec*shadow;
}