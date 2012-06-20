#version 130

precision highp float;

uniform vec3 in_light;

uniform mat4 modelview_matrix;

in vec3 in_position;
in vec2 in_texture;

uniform vec3 defDirection;
uniform vec3 defColor;
uniform mat4 defMatrix;
uniform mat4 defInnerMatrix;

out vec2 v_texture;
out vec3 v_light;

void main(void)
{
	gl_Position = vec4(in_position, 1);
	v_texture = 1-in_texture;
	
	v_light = normalize((modelview_matrix * vec4(defDirection, 0)).xyz);
	
	
}