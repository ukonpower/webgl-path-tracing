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
	vec3 position;
	vec3 normal;
	bool hit;
	float distance;
	int material;
};

struct Sphere {
	vec3 position;
	float radius;
	float material;
};

struct Plane {
	vec3 position;
	vec3 normal;
	float material;
};

void intersectionPlane( inout Intersection intersection, Ray ray, Plane plane ) {

	vec3 s = ray.origin - plane.position;

	float dn = dot( ray.direction, plane.normal );

	if( dn != 0.0 ) {

		float sn = dot( s, plane.normal );
		float t = - ( sn / dn );

		if( t > EPS && t < intersection.distance ) {

			intersection.hit = true;
			intersection.position = ray.origin + ray.direction * t;
			intersection.distance = t;
			intersection.normal = plane.normal;

		}
		
	}
	
}

// http://viclw17.github.io/2018/07/16/raytracing-ray-sphere-intersection/

void intersectionSphere( inout Intersection intersection, Ray ray, Sphere sphere ) {

	vec3 s = ray.origin - sphere.position;
	float a = dot( ray.direction, ray.direction );
	float b = 2.0 * dot( s, ray.direction );
	float c = dot( s, s ) - sphere.radius * sphere.radius;
	float d = b * b - 4.0 * a * c;
	float t = ( -b - sqrt( d ) ) / ( 2.0 * a );

	if( d > EPS && t < intersection.distance ) {

		intersection.hit = true;
		intersection.position = ray.origin + ray.direction * t;
		intersection.distance = t;
		intersection.normal = normalize( intersection.position - sphere.position );
		
	}
	
}

void shootRay( inout Intersection intersection, Ray ray ) {

	intersection.hit = false;
	intersection.distance = INF;
	intersection.material = 0;

	Plane plane;
	plane.position = vec3( 0, 0, 0 );
	plane.normal = vec3( 0, 1, 0 );
	intersectionPlane( intersection, ray, plane );

	Sphere sphere;
	sphere.radius = 0.5;
	sphere.position = vec3( 0, 0.5, 0 );
	intersectionSphere( intersection, ray, sphere );


}

vec3 radiance( Ray ray ) {

	Intersection intersection;

	// for ( int i = 0; i < MAX_BOUNCE; i++ ) {

		shootRay( intersection, ray );

	// }

	return intersection.hit ? intersection.normal : vec3( 0.0 );
	
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