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
uniform sampler2D emissionBuffer;
uniform sampler2D materialBuffer;
uniform sampler2D normalBuffer;
uniform sampler2D depthBuffer;
uniform sampler2D backNormalBuffer;
uniform sampler2D backDepthBuffer;


bool debug = false;
varying vec2 vUv;

#define MAX_BOUNCE 20

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

#define MAX_STEP 40

int shootRay( inout Intersection intersection, inout Ray ray, int bounce ) {

	intersection.hit = false;
	intersection.distance = INF;
	intersection.position = ray.origin;

	for( int i = 0; i < MAX_STEP; i++ ) {

		intersection.memPos = intersection.position;
		intersection.position += ray.direction * 0.5;

		vec3 middlePos = ( intersection.memPos + intersection.position ) / 2.0;

		vec4 middleClip = cameraProjectionMatrix * vec4( middlePos, 1.0 );
		middleClip.xyz /= middleClip.w;

		vec4 currentClip = cameraProjectionMatrix * vec4( intersection.position, 1.0 );
		currentClip.xyz /= currentClip.w;

		vec4 memClip = cameraProjectionMatrix * vec4( intersection.memPos, 1.0 );
		memClip.xyz /= memClip.w;

		vec2 pickUV = (middleClip.xy) * 0.5 + 0.5;
		vec4 texDepthFront = texture2D( depthBuffer, pickUV );
		vec4 texDepthBack = texture2D( backDepthBuffer, pickUV );

		float middleDepthFront = texDepthFront.x / texDepthFront.w;
		float middleDepthBack = texDepthBack.x / texDepthBack.w;
		float currentDepth = currentClip.z;
		float memDepth = memClip.z;

		//当たり判定
		// if( (currentDepth >= middleDepthFront && middleDepthFront >= memDepth && middleDepthFront != 0.0 && bounce != 2 ) || ( bounce == 1 && i == 0 ) ) {
		if(
			(( currentDepth >= middleDepthFront && middleDepthFront >= memDepth ) || 
			( ( currentDepth >= middleDepthFront && middleDepthFront <= memDepth ) ) && ( currentDepth <= middleDepthBack && middleDepthBack >= memDepth ) ) &&
			middleDepthFront != 0.0 
		) {

			Material mat;
			mat.albedo = texture2D( albedoBuffer, pickUV ).xyz;
			mat.emission = texture2D( emissionBuffer, pickUV ).xyz;
			vec4 matTex = texture2D( materialBuffer, pickUV );
			mat.roughness = matTex.x;
			mat.metalness = matTex.y;

			intersection.normal = normalize( texture2D( normalBuffer, pickUV ).xyz * 2.0 - 1.0 );

			vec3 p = ( cameraProjectionMatrixInverse * vec4( (pickUV * 2.0 - 1.0) * texDepthFront.w, middleDepthFront, texDepthFront.w ) ).xyz;
			intersection.position = p;
			intersection.material = mat;

			intersection.hit = true;

			if( false ) {

				debug = true;
				#define DEBUG_NUM 1
				intersection.material.albedo = vec3( currentDepth >= middleDepthFront, middleDepthFront >= memDepth, 0.0 );
				

			}

			break;

		}

	}


	if( intersection.hit ) {

		float seed =  frame * 0.001 + float( bounce );
		vec2 noise = vec2( random( vUv + sin( seed ) ), random( vUv - cos( seed ) ) );

		ray.origin = intersection.position;

		if( random( vUv * 10.0 + sin( time + float( frame ) + seed ) ) > 0.5 * ( 1.0 - intersection.material.roughness * ( 1.0 - intersection.material.metalness )  ) + intersection.material.metalness * 0.5 ) {
			
			ray.direction = diffuse( intersection, noise );
			
			return 0;
			
		} else {

			ray.direction = ggx( intersection, ray, noise );
			return 1;

		}

	} else {

		intersection.material.emission = vec3( 1.0 );

	}

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

	vec3 emission = memEmission[ MAX_BOUNCE - 1 ];
	vec3 col;

	for ( int i = MAX_BOUNCE - 2; i >= 0 ; i-- ) {

		if ( memDir[ i ] > 0 ) {

			//ggx
			col *= mix( vec3( 1.0 ), memAlbedo[i], memMetalness[ i ] );

		} else {
			
			//diffuse
			col *= mix( vec3( 0.0 ), memAlbedo[i], 1.0 - memMetalness[ i ] );

		}

		col += memEmission[ i ];

	}

	if( debug ) {

		col = memAlbedo[DEBUG_NUM];
		
	}

	return col;
	
	
}

void main( void ) {
	
	vec4 befTex = texture2D( backBuffer, vUv ) * min( frame, 1.0 ) ;

	vec2 uv = vUv * 4.0;
	vec4 depth = texture2D( backNormalBuffer, uv );
	vec2 mask = step( vec2( 0.25 ), vUv );

	Ray ray;
	// ray.origin = cameraPosition;
	// ray.direction = ( cameraProjectionMatrixInverse * vec4( vUv * 2.0 - 1.0, 1.0, 1.0 ) ).xyz;
	
	ray.origin = vec3( 0.0, 0.0, 0.0 );
	ray.direction = ( cameraProjectionMatrixInverse * vec4( vUv * 2.0 - 1.0, 1.0, 1.0 ) ).xyz;
	ray.direction = normalize( ray.direction );

	float clip = ( 1.0 - mask.x ) * ( 1.0 - mask.y );
	vec4 o = vec4( ( befTex.xyz + radiance( ray ) ) , 1.0 ) * ( 1.0 - clip );
	// vec4 o = vec4( ( befTex.xyz + vec3( ray.direction.xyz ) ) , 1.0 ) * ( 1.0 - clip );
	gl_FragColor = o;

	gl_FragColor += mix( vec4(0.0), vec4( depth ), clip ) + befTex * clip;

}