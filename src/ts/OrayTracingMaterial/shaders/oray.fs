uniform float renderType;
uniform vec3 albedo;
uniform float roughness;
uniform float metalness;

varying vec2 vUv;
varying vec3 vNormal;
varying float vDepth;
varying vec4 vPos;

void main( void ) {

	if( renderType == 0.0 ) {

		//albedo
		gl_FragColor = vec4( albedo, 0.0 );
		
	} else if ( renderType == 1.0 ) {

		//material
		gl_FragColor = vec4( roughness, metalness, 0.0, 0.0 );
		
	} else if ( renderType == 2.0 ) {

		//normal
		gl_FragColor = vec4( vNormal * 0.5 + 0.5, 0.0 );
		
	} else if ( renderType == 3.0 ) {

		//depth
		gl_FragColor = vec4( ( vPos.z / vPos.w + 1.0 ) * 0.5 );
		
	}


}