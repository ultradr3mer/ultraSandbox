#version 130
precision mediump float;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform sampler2D Texture3;
uniform vec2 in_vector;
in vec2 vTextureCoord;

out vec4 out_frag_color;

void main() {
	out_frag_color = texture(Texture3, vTextureCoord)*(texture(Texture1, vTextureCoord)+texture(Texture2, vTextureCoord));
	out_frag_color = texture(Texture1, vTextureCoord);
}