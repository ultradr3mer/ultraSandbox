#version 130
precision mediump float;
uniform sampler2D Texture1;
in vec2 v_texture;


out vec4 out_frag_color;
uniform vec2 in_rendersize;
uniform vec2 in_vector;

void main() {
	vec2 rastersize = 1/in_rendersize;
	
	vec4 col = vec4(0,0,0,0);
	
	float samples = in_vector.x;
	for(float i = -samples; i <= samples; i ++)
	{
		col += texture(Texture1, v_texture + vec2(rastersize.x*i,0));
	}
	
	samples = in_vector.y;
	for(float i = -samples; i <= samples; i ++)
	{
		col += texture(Texture1, v_texture + vec2(0,rastersize.y*i));
	}
	
	
	out_frag_color = col/((in_vector.x+in_vector.y)*2.0);
}