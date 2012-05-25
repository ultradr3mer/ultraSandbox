#version 130

precision highp float;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;
uniform mat4 model_matrix;
uniform mat4 rotation_matrix;

uniform vec3 in_light;
uniform vec3 in_eyepos;

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
out vec3 light;

void main(void)
{
  //works only for orthogonal modelview
  //normal = normalize((modelview_matrix * vec4(in_normal, 0)).xyz);
    
	v_normal = normalize((rotation_matrix * vec4(in_normal, 0)).xyz);
  
	g_pos = model_matrix * rotation_matrix * vec4(in_position, 1);
	gl_Position = projection_matrix * modelview_matrix * g_pos;
	
	v_eyedirection = normalize(g_pos.xyz - in_eyepos);
  
	v_texture = in_texture;
	
	v_tangent = normalize((rotation_matrix * vec4(in_tangent, 0)).xyz);
	
	v_bnormal = normalize(cross(v_normal, v_tangent));
  
	light = in_light;
}