#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;

in vec3 normal;
in vec2 v_texture;
in vec3 light;
uniform sampler2D Texture1;

out vec4 out_frag_color;

void main(void)
{
  //vec4 Color = texture(Texture1, v_texture)*vec4(0.8,0.5,0.8,1.0);
  vec4 Color = texture(Texture1, v_texture)*vec4(0.5,0.7,1.0,1.0);
  float diffuse = clamp(dot(light, normal), 0.0, 1.0);
  out_frag_color = vec4(in_lightambient + diffuse * in_lightcolor, 1.0)*Color;
  //out_frag_color = vec4(normal,1);
}