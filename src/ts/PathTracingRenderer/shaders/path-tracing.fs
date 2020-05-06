uniform sampler2D backBuffer;

varying vec2 vUv;

void main( void ) {
	
	vec4 befTex = texture2D( backBuffer, vUv );

	gl_FragColor = befTex + 0.01;

}