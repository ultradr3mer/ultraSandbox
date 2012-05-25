#version 130
precision mediump float;
uniform mat4 mRMatrix;
uniform mat4 uMVPMatrix;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform sampler2D Texture3;
uniform vec3 in_lightambient;
uniform vec3 in_lightcolor;
uniform vec2 in_vector;
uniform int in_pass;
uniform float in_waterlevel;

in vec2 vTextureCoord;
in vec3 vNormal;
in vec3 vLight_Normal;
in vec4 vGPos;
in vec3 viewNormal;

float fade_angle = 0.55;
float fade_angle_b = 0.50;
float fade_width = fade_angle_b - fade_angle;

out vec4 out_frag_color;

vec4 surface_color(){
		vec3 N = vec3(texture(Texture2, vTextureCoord).xyz * 2.0 - 1.0);
		N = normalize(vec3(vec4(N[0],-N[2],N[1],1)));
		
		float Diffuse = clamp(dot(N, normalize(vLight_Normal)), 0.0, 1.0);
		
		
		vec4 Diff_color = vec4(0.5,0.7,1.0,1.0);
			
		if (N[1] < fade_angle) {
			if (N[1] > fade_angle_b) {
				float fade = (fade_angle_b-N[1])/fade_width;
				Diff_color = Diff_color*fade+texture(Texture1, vTextureCoord*8.0)*(1.0-fade);
			} else {
				Diff_color = texture(Texture1, vTextureCoord*8.0);
			}
		}

		return Diff_color*vec4((in_lightcolor*Diffuse)+in_lightambient,1);
}

void main() {
	if (in_pass == 0.0) {
		if( vGPos[1]-in_waterlevel < 0.0){
			vec4 surface_color = surface_color();
			float visibility = 1.0+(vGPos[1]-in_waterlevel)/in_waterlevel/0.7;
			
			surface_color[3] = visibility;

			out_frag_color = surface_color;
		} else {
			out_frag_color = surface_color();
		}
	}
	if (in_pass == 1.0) {
		if( vGPos[1]-in_waterlevel < 0.0 ){
			discard;
		} else {
			out_frag_color = surface_color();
		}
	}
}