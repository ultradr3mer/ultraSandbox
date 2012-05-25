#version 130

const int no_lights = 6;

precision highp float;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;
uniform mat4 model_matrix;
uniform mat4 rotation_matrix;

uniform mat4 model_matrix2;
uniform mat4 rotation_matrix2;

uniform vec3 in_light;
uniform vec3 in_eyepos;

uniform float in_time;

uniform mat4 shadow_matrix[no_lights];

uniform sampler2D base2Texture;

in vec3 in_normal;
in vec3 in_position;
in vec2 in_texture;
in vec3 in_tangent;

out vec4 g_pos;
out vec3 v_eyedirection;
out vec3 v_normal;
out vec2 v_texture;
out vec3 v_tangent;
out vec3 v_bnormal;
out vec3 v_ss_normal;
out vec3 v_ss_tangent;
out vec3 v_ss_bnormal;
out vec3 light;
out float v_depth;

out vec4 v_s_map_pos[no_lights];


void main(void)
{
  //works only for orthogonal modelview
  //normal = normalize((modelview_matrix * vec4(in_normal, 0)).xyz);
  
	float blend = - (in_position.z+0.65)/4.35;
	
	v_texture = in_time * vec2(0.2,1.0) + in_texture;
	
	vec3 offset = texture(base2Texture , v_texture).xyz * 2.0 - 1.0;
	float offset_scale = (blend - 0.5)*2;
	offset_scale = offset_scale*sign(offset_scale);
	offset_scale = 1-offset_scale;
	
	vec3 mod_position = in_position + offset * offset_scale * 0.5;
	
	blend = clamp(pow(blend,3), 0.001, 1.0);
    
	g_pos = model_matrix * rotation_matrix * vec4(mod_position, 1);
	vec4 g_pos2 = model_matrix2 * rotation_matrix2 * vec4(mod_position, 1);
	
	g_pos = g_pos * (1 - blend) + g_pos2 * blend;
	
	gl_Position = projection_matrix * modelview_matrix * g_pos;
	
	v_eyedirection = normalize(g_pos.xyz - in_eyepos);
	
	v_normal = normalize((rotation_matrix * vec4(in_normal, 0)).xyz);
	v_tangent = normalize((rotation_matrix * vec4(in_tangent, 0)).xyz);
	v_bnormal = normalize(cross(v_normal, v_tangent));
}