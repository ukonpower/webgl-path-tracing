uniform vec2 dataSize;
uniform mat4 cameraMatrixWorld;
uniform mat4 cameraProjectionMatrix;
uniform mat4 cameraProjectionMatrixInverse;
uniform mat4 projectionMatrix;

uniform float roughness;
uniform float metalness;
uniform vec3 albedo;

uniform float time;
uniform float frame;

uniform sampler2D backBuffer;
uniform sampler2D albedoBuffer;
uniform sampler2D materialBuffer;
uniform sampler2D normalBuffer;
uniform sampler2D depthBuffer;

varying vec2 vUv;

#define MAX_BOUNCE 1

$constants
$random

const float INF = 1e+10;
const float EPS = 1e-5;

struct Ray {
	vec3 origin;
	vec3 direction;
};

struct Material {
	vec3 albedo;
	vec3 emission;
	float roughness;
	float metalness;
};

struct Intersection {
	vec3 position;
	vec3 normal;
	vec3 memPos;
	bool hit;
	float distance;
	Material material;
};

struct Sphere {
	vec3 position;
	float radius;
	Material material;
};

struct Plane {
	vec3 position;
	vec3 normal;
	Material material;
};

vec3 random3D( vec2 p, float seed ) {

	return vec3(
		random( p + seed ),
		random( p + seed + 100.0 ),
		random( p + seed + 303.2)
	);
	
}

//http://project-asura.com/blog/archives/3124
vec3 ggx( Intersection intersection, Ray ray, vec2 noise )
{

	vec3 normal = intersection.normal;
	float roughness = intersection.material.roughness;

    float a = roughness * roughness;

    float phi = 2.0 * PI * noise.x;
    float cosTheta = sqrt( ( 1.0  - noise.y ) / ( 1.0  + ( a * a - 1.0 ) * noise.y ) );
    float sinTheta = sqrt( 1.0  - cosTheta * cosTheta );
    
    vec3 H;
    H.x = sinTheta * cos( phi );
    H.y = sinTheta * sin( phi );
    H.z = cosTheta;
    
    vec3 upVector = abs( normal.z ) < 0.999 ? vec3( 0, 0, 1 ) : vec3( 1, 0, 0 );
    vec3 tangentX = normalize( cross( upVector , normal ) );
    vec3 tangentY = cross( normal, tangentX );

    return reflect( ray.direction, tangentX * H.x + tangentY * H.y + normal * H.z );

}

vec3 diffuse( Intersection intersection, vec2 noise ) {

	vec3 normal = intersection.normal;
	
	float r = sqrt( noise.x );
	float theta = TPI * noise.y;

	vec3 tDir = vec3( r * cos( theta ), r * sin( theta ), sqrt( 1.0 - noise.x ) );
	vec3 tangent = normalize( cross( normal, abs( normal.x ) > EPS ? vec3( 0.0, 1.0, 0.0 ) : vec3( 1.0, 0.0, 0.0 ) ) );
	vec3 binormal = cross( tangent, normal );
	
	return tangent * tDir.x + binormal * tDir.y + normal * tDir.z;

}

#define MAX_STEP 10

int shootRay( inout Intersection intersection, inout Ray ray, int bounce ) {

	intersection.hit = false;
	intersection.distance = INF;
	intersection.position = ray.origin;

	for( int i = 0; i < MAX_STEP; i++ ) {

		intersection.memPos = intersection.position;
		intersection.position += ray.direction  * 2.0;

		vec3 middlePos = ( intersection.memPos + intersection.position ) / 2.0;

		vec4 middleClip = cameraProjectionMatrix * vec4( middlePos, 1.0 );
		middleClip.xyz /= middleClip.w;

		vec4 currentClip = cameraProjectionMatrix * vec4( intersection.position, 1.0 );
		currentClip.xyz /= currentClip.w;

		vec4 memClip = cameraProjectionMatrix * vec4( intersection.memPos, 1.0 );
		memClip.xyz /= memClip.w;

		vec4 texDepth = texture2D( depthBuffer, (middleClip.xy) * 0.5 + 0.5 );
		float currentDepth = currentClip.z;
		float memDepth = memClip.z;

		// if( i == MAX_STEP - 1 ) {

		// 	Material mat;
		// 	mat.albedo = vec3( texDepth );

		// 	intersection.material = mat;
		// 	intersection.hit = false;

		// 	return 0;

		// }

		//当たり判定
		if( currentDepth > texDepth.x && texDepth.x > memDepth) {

			Material mat;
			mat.albedo = vec3( texDepth );

			intersection.material = mat;
			intersection.hit = false;

			return 0;

		}

	}

	Material mat;
	mat.albedo = vec3( 0.0, 0.0, 0.0 );
	intersection.material = mat;
	// if( intersection.hit ) {

	// 	float seed =  frame * 0.001 + float( bounce );
	// 	vec2 noise = vec2( random( vUv + sin( seed ) ), random( vUv - cos( seed ) ) );

	// 	ray.origin = intersection.position;

	// 	if( random( vUv * 10.0 + sin( time + float( frame ) + seed ) ) > 0.5 * ( 1.0 - intersection.material.roughness * ( 1.0 - intersection.material.metalness )  ) + intersection.material.metalness * 0.5 ) {
			
	// 		ray.direction = diffuse( intersection, noise );
			
	// 		return 0;
			
	// 	} else {

	// 		ray.direction = ggx( intersection, ray, noise );

	// 		return 1;

	// 	}

	// } else {

	// 	intersection.material.emission = vec3( 1.0 );
	// 	intersection.material.emission = vec3( 0.0 );

	// }

	return 0;

}

vec3 radiance( inout Ray ray ) {

	Intersection intersection;

	// Material memMaterial[MAX_BOUNCE];
	float memMetalness[MAX_BOUNCE];
	vec3 memAlbedo[MAX_BOUNCE];
	vec3 memEmission[MAX_BOUNCE];
	int memDir[MAX_BOUNCE];

	int bounce;
	
	for ( int i = 0; i < MAX_BOUNCE; i++ ) {

		memDir[i] = shootRay( intersection, ray, i );
		memAlbedo[i] = intersection.material.albedo;
		memEmission[i] = intersection.material.emission;
		memMetalness[i] = intersection.material.metalness;

		if( !intersection.hit ) {

			bounce = i;

			break;
			
		}
	}

	return memAlbedo[0];

	// vec3 emission = memEmission[ MAX_BOUNCE - 1 ];
	// vec3 col;

	// for ( int i = MAX_BOUNCE - 2; i >= 0 ; i-- ) {

	// 	if ( memDir[ i ] > 0 ) {

	// 		//ggx
	// 		col *= mix( vec3( 1.0 ), memAlbedo[i], memMetalness[ i ] );

	// 	} else {
			
	// 		//diffuse
	// 		col *= mix( vec3( 0.0 ), memAlbedo[i], 1.0 - memMetalness[ i ] );

	// 	}

	// 	col += memEmission[ i ];

	// }

	// return col;
	
}

void main( void ) {
	
	vec4 befTex = texture2D( backBuffer, vUv ) * min( frame, 1.0 ) ;

	vec2 uv = vUv * 4.0;
	vec4 depth = texture2D( depthBuffer, uv );
	vec2 mask = step( vec2( 0.25 ), vUv );

	Ray ray;
	// ray.origin = cameraPosition;
	// ray.direction = ( cameraProjectionMatrixInverse * vec4( vUv * 2.0 - 1.0, 1.0, 1.0 ) ).xyz;
	
	ray.origin = vec3( 0.0 );
	ray.direction = (  cameraProjectionMatrixInverse * vec4( vUv * 2.0 - 1.0, 1.0, 1.0 ) ).xyz;
	ray.direction = normalize( ray.direction );

	vec4 o = vec4( ( befTex.xyz + radiance( ray ) ) , 1.0 );
	gl_FragColor = o;

	gl_FragColor = mix(gl_FragColor, vec4( depth ), ( 1.0 - mask.x ) * ( 1.0 - mask.y ) );



}