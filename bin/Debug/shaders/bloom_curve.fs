#version 130
precision mediump float;
uniform sampler2D Texture1;
in vec2 v_texture;

uniform float bloomExp = 2.4;
uniform float bloomStrength = 2;

out vec4 out_frag_color;
uniform vec2 in_rendersize;
uniform vec2 in_vector;

void main() {
	vec2 rastersize = 1/in_rendersize*0.25;
	
	vec4 col = vec4(0,0,0,0);
	
	float samples = 2;
	
	for(float i = -samples; i <= samples; i ++)
	{
		col += texture(Texture1, v_texture + vec2(rastersize.x*i,rastersize.y*i));
	}
	for(float i = -samples; i <= samples; i ++)
	{
		col += texture(Texture1, v_texture + vec2(rastersize.x*i,-rastersize.y*i));
	}
	
	col /= (samples*4.0);
	
	col.r = pow(col.r,bloomExp)*bloomStrength;
	col.g = pow(col.g,bloomExp)*bloomStrength;
	col.b = pow(col.b,bloomExp)*bloomStrength;
	
	out_frag_color = col;
}

