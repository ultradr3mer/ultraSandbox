#version 130
precision mediump float;
uniform sampler2D Texture1;

uniform mat4 modelview_matrix;

in vec2 v_texture;


out vec4 out_frag_color;
uniform vec2 in_screensize;


float PI = 3.14159265;
int samples = 5; //samples on the first ring (5-10)
int rings = 4; //ring count (2-6)

void main() {
	vec4 info = texture(Texture1, v_texture);

	vec3 TexN = info.rgb * 2 -1;
	
	TexN = normalize((modelview_matrix * vec4(TexN, 0)).xyz);
	out_frag_color.rgb = TexN.rgb * 0.5 + 0.5;
	out_frag_color.a = info.a;
}