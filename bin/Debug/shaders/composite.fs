#version 130

precision highp float;

uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;

uniform vec2 in_screensize;

in vec4 g_pos;
in vec3 v_normal;
in vec2 v_texture;
in vec3 v_tangent;
in vec3 v_bnormal;
in vec3 light;

uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform sampler2D Texture3;
uniform sampler2D Texture4;
uniform sampler2D Texture5;
uniform sampler2D Texture6;


out vec4 out_frag_color;

float PI = 3.14159265;
int samples = 4; //samples on the first ring (5-10)
int rings = samples; //ring count (2-6)

vec4 sample(vec2 coo)
{
	return texture(Texture4, coo);
}

void main(void)
{
	vec4 selectionTerm = texture(Texture4, v_texture)*(1-texture(Texture3, v_texture))*vec4(0.8,0.3,0.8,1.0);
	vec4 sceneTerm  = texture(Texture1, v_texture);
		
	vec4 dofTerm = texture(Texture5, v_texture);
	if(dofTerm.a != 0){
			sceneTerm = sceneTerm * (1 - dofTerm.a) + dofTerm * dofTerm.a;
	}
	
  	out_frag_color = sceneTerm+texture(Texture2, v_texture)+selectionTerm;
	//out_frag_color = ((texture(Texture1, v_texture)+texture(Texture2, v_texture)))+selection();
	//out_frag_color = texture(Texture1, v_texture)+texture(Texture2, v_texture)+texture(Texture3, v_texture);
	//out_frag_color =  vec4(1,1,1,1)*texture(Texture5, v_texture).a;
	//out_frag_color +=  texture(Texture6,v_texture);
	//out_frag_color = vec4(1,1,1,1)*texture(Texture6, v_texture).a;
	//out_frag_color.a = 1;
}


