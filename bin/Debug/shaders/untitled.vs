#version 130

precision highp float;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;
uniform mat4 model_matrix;

uniform vec3 in_light;

in vec3 in_normal;
in vec3 in_position;
in vec2 in_texture;

out vec4 g_pos;
out vec3 normal;
out vec2 v_texture;
out vec3 light;

void main(void)
{
  //works only for orthogonal modelview
  //normal = normalize((modelview_matrix * vec4(in_normal, 0)).xyz);
  
  normal = normalize(in_normal);
  
  light = normalize(in_light);

  v_texture = in_texture;
  
  g_pos = model_matrix * vec4(in_position, 1);
  gl_Position = projection_matrix * modelview_matrix * g_pos;
}