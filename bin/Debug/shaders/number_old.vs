precision mediump float;
uniform mat4 uMVPMatrix;
uniform mat4 mRMatrix;
uniform vec3 Light;
uniform int Digit;
attribute vec4 Position;
attribute vec3 Normal;
attribute vec2 TextureCoord;
varying vec2 vTextureCoord;

void main() {
	vTextureCoord = TextureCoord;
	
	vec4 shift = vec4(-float(Digit)*2.0,0.0,0.0,0.0);

	gl_Position = uMVPMatrix * (Position+shift);
}