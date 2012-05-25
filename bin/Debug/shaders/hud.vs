#version 130

precision highp float;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;
uniform mat4 model_matrix;
uniform mat4 rotation_matrix;

uniform vec2 in_hudsize;
uniform vec2 in_hudpos;

in vec3 in_position;
in vec2 in_texture;

out vec2 v_texture;

void main(void)
{
  //works only for orthogonal modelview
  //normal = normalize((modelview_matrix * vec4(in_normal, 0)).xyz);
	
	gl_Position = vec4(in_hudpos,0,0)+vec4(in_position, 1)*vec4(in_hudsize,1,1);
  
	v_texture = vec2(1-in_texture.x,in_texture.y);
}