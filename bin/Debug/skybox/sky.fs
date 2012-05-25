#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightsun;

in vec3 normal;
in vec2 v_texture;
uniform sampler2D Texture1;

out vec4 out_frag_color;

void main(void)
{
  vec4 Color = texture(Texture1, v_texture);
  //float diffuse = clamp(dot(lightVecNormalized, normalize(normal)), 0.0, 1.0);
  out_frag_color = Color*vec4(in_lightsun * 2,1);
}