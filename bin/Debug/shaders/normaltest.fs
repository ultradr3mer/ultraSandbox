#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;
uniform vec3 in_lightdir;

in vec4 g_pos;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;
in vec3 light;

uniform sampler2D Texture1;
uniform sampler2D Texture2;

out vec4 out_frag_color;

float getlight(vec3 N){
	float brightness = 2;

	float ang_hardness = 10;
	float ang_limit = 0.7;
	float lenth_mult = 0.2;
		
	vec3 light_vec = normalize(light-g_pos.xyz);
	vec3 light_dir = normalize(in_lightdir);
	
	float light_angular = clamp((dot(light_vec,-light_dir)-ang_limit)*ang_hardness, 0.0, 1.0);
	float light_diffuse = clamp(dot(light_vec, N), 0.0, 1.0);
	float light_distance = 1/exp2(length(light-g_pos.xyz)*lenth_mult);
	
	return light_angular*light_diffuse*light_distance*brightness;
}

void main(void)
{
	vec4 NTexValue = texture(Texture1, v_texture) * 2.0 - 1.0;
	vec3 N = normalize(NTexValue[2] * v_normal + NTexValue[0] * v_tangent + NTexValue[1] * v_bnormal);
	
	float light_final = getlight(N);

  //vec4 Color = texture(Texture1, v_texture)*vec4(0.8,0.5,0.8,1.0);
  vec4 Color = texture(Texture2, v_texture)*vec4(0.5,0.7,1.0,1.0);
  out_frag_color = vec4(in_lightambient + light_final * in_lightcolor, 1.0)*Color;
  out_frag_color = vec4(N,1);
}