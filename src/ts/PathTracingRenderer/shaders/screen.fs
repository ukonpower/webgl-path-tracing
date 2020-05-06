uniform sampler2D renderResult;

varying vec2 vUv;

void main( void ) {

	gl_FragColor = texture2D( renderResult, vUv );

}