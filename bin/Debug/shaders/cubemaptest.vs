#version 130

const int no_lights = 10;

precision lowp float;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;
uniform mat4 model_matrix;
uniform mat4 rotation_matrix;

uniform vec3 in_light;
uniform vec3 in_eyepos;

uniform int in_no_lights;

struct Light
{
    bool active;
    vec3 position;
	vec3 direction;
    vec3 color;
	mat4 view_matrix;
	bool texture;
};

struct SunLight
{
    bool active;
	vec3 direction;
    vec3 color;
	mat4 view_matrix;
	mat4 inner_view_matrix;
};

uniform Light lightStructs[no_lights];
uniform SunLight sunLightStruct;

in vec3 in_normal;
in vec3 in_position;
in vec2 in_texture;
in vec3 in_tangent;

out vec4 v_sun_map_pos;
out vec4 v_inner_sun_map_pos;
out vec4 g_pos;
out vec3 v_eyedirection;
out vec3 v_normal;
out vec2 v_texture;
out vec3 v_tangent;
out vec3 v_bnormal;
out vec3 v_ss_normal;
out vec3 v_ss_tangent;
out vec3 v_ss_bnormal;
out float v_depth;

out vec4 v_s_map_pos[no_lights];

uniform bool use_alpha;


void main(void)
{
  //works only for orthogonal modelview
  //normal = normalize((modelview_matrix * vec4(in_normal, 0)).xyz);
    

	g_pos = model_matrix * rotation_matrix * vec4(in_position, 1);
	gl_Position = projection_matrix * modelview_matrix * g_pos;
	
	v_eyedirection = normalize(g_pos.xyz - in_eyepos);
  
	v_texture = in_texture;
	
	v_normal = normalize((rotation_matrix * vec4(in_normal, 0)).xyz);
	v_tangent = normalize((rotation_matrix * vec4(in_tangent, 0)).xyz);
	v_bnormal = normalize(cross(v_normal, v_tangent));
	
	v_texture = in_texture;
	
	if(use_alpha)
	{
		v_ss_normal = normalize((modelview_matrix * rotation_matrix * vec4(in_normal, 0)).xyz);
		v_ss_tangent = normalize((modelview_matrix * rotation_matrix * vec4(in_tangent, 0)).xyz);
		v_ss_bnormal = normalize(cross(v_ss_normal, v_ss_tangent));
	}
  	
	v_depth = gl_Position.a;
	
	for(int i = 0; i < no_lights; i++) {
		if(i < in_no_lights && lightStructs[i].active)
			v_s_map_pos[i] = lightStructs[i].view_matrix * g_pos;
	}
	
	v_sun_map_pos = sunLightStruct.view_matrix * g_pos;
	v_inner_sun_map_pos = sunLightStruct.inner_view_matrix * g_pos;
}