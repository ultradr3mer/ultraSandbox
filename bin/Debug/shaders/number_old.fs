precision mediump float;
uniform float Value;
uniform sampler2D Texture1;
uniform sampler2D Texture2;
uniform sampler2D Texture3;
uniform vec3 lightColor;
varying vec2 vTextureCoord;

void main() {
	float coord_y = vTextureCoord[1];
	coord_y = coord_y*0.2-0.05+Value*0.1;
	vec4 Diff_collor = texture2D(Texture1, vec2(vTextureCoord[0],coord_y))+texture2D(Texture2, vTextureCoord);
	gl_FragColor = Diff_collor*vec4(0.5,0.7,1.0,1.0)*texture2D(Texture3, vTextureCoord)*2.0;
}