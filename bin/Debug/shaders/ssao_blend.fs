#version 130
precision mediump float;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
in vec2 v_texture;


out vec4 out_frag_color;
uniform vec2 in_screensize;


float PI = 3.14159265;
int samples = 5; //samples on the first ring (5-10)
int rings = 4; //ring count (2-6)

void main() {
	float ao = texture(Texture1, v_texture).a;
	if(ao != 0){
		float ao_factor = texture(Texture1, v_texture).a*2;
		out_frag_color = texture(Texture2, v_texture)*ao_factor;
		out_frag_color.a = 1;
	} else {
		discard;
	}	
	//out_frag_color = vec4(1,1,1,1)*ao;
	//out_frag_color = texture(Texture2, v_texture);
	//out_frag_color.a = 1;
}