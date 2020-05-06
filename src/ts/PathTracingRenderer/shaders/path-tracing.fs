uniform vec2 dataSize;
uniform mat4 cameraMatrixWorld;
uniform mat4 cameraProjectionMatrixInverse;
uniform sampler2D backBuffer;

varying vec2 vUv;

#define MAX_BOUNCE 2

$constants

const float INF = 1e+10;
const float EPS = 1e-5;

struct Ray {
	vec3 origin;
	vec3 direction;
};

struct Intersection {
	bool hit;
	float dist;
	vec3 normal;
	int material;
};

struct Sphere {
	vec3 position;
	float radius;
	float material;
};

// http://viclw17.github.io/2018/07/16/raytracing-ray-sphere-intersection/

void intersectionSphere( inout Intersection intersection, Ray ray, Sphere sphere ) {

	vec3 oc = ray.origin - sphere.position;
	float a = dot( ray.direction, ray.direction );
	float b = 2.0 * dot( oc, ray.direction );
	float c = dot( oc, oc ) - sphere.radius * sphere.radius;
	float discriminant = b * b - 4.0 * a * c;

	if( discriminant > 0.01 ) {

		intersection.hit = true;
		
	}
	
}

void shootRay( inout Intersection intersection, Ray ray ) {

	intersection.hit = false;
	intersection.dist = INF;
	intersection.material = 0;

	Sphere sphere;
	sphere.radius = 1.0;
	sphere.position = vec3( 0, 0, 0 );

	intersectionSphere( intersection, ray, sphere );

}

vec3 radiance( Ray ray ) {

	Intersection intersection;

	// for ( int i = 0; i < MAX_BOUNCE; i++ ) {

		shootRay( intersection, ray );

	// }

	return intersection.hit ? vec3( 1.0 ) : vec3( 0.0 );
	
}

void main( void ) {
	
	vec4 befTex = texture2D( backBuffer, vUv );

	Ray ray;
	ray.origin = cameraPosition;
	ray.direction = ( cameraMatrixWorld * cameraProjectionMatrixInverse * vec4( vUv * 2.0 - 1.0, 1.0, 1.0 ) ).xyz;
	ray.direction = normalize( ray.direction );

	vec4 o = vec4( radiance( ray ), 1.0 );

	gl_FragColor = o;

}