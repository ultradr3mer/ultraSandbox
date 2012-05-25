#version 130

precision highp float;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;
uniform mat4 model_matrix;
uniform mat4 rotation_matrix;

uniform vec3 in_light;

uniform vec2 in_vector;

in vec3 in_position;
in vec2 in_texture;

out vec2 v_texture;

void main(void)
{
  //works only for orthogonal modelview
  //normal = normalize((modelview_matrix * vec4(in_normal, 0)).xyz);
  
  	vec4 pos = vec4(in_position, 1);
	pos.x = (pos.x+1+in_vector[0]*2)/in_vector[1]-1;
	pos.z = 0.99;

	gl_Position = pos ;
  
	v_texture = 1-in_texture;
}