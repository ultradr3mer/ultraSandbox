#version 130
uniform int in_no_lights;

uniform int curLight;
uniform vec2 in_rendersize;

out vec4 out_frag_color;

vec2 screenpos(){
	return gl_FragCoord.xy/in_rendersize;
}

void main(void)
{
	vec2 screenpos = screenpos();
	
	float leftclip = curLight/float(in_no_lights);
	float rightclip = (curLight+1)/float(in_no_lights);
	
	if(screenpos.x > rightclip || screenpos.x < leftclip){
		discard;
	}else{
		out_frag_color = vec4(1,1,1,gl_FragCoord.z);
	}
}