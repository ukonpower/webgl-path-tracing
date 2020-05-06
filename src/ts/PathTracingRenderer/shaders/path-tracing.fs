uniform vec2 dataSize;
uniform mat4 cameraMatrixWorld;
uniform mat4 cameraProjectionMatrixInverse;

uniform float time;
uniform float frame;

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

struct Material {
	vec3 color;
	vec3 emmission;
	float roughness;
	float metalness;
};

struct Intersection {
	vec3 position;
	vec3 normal;
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

vec3 importanceSampleGGX( vec2 Xi, float Roughness, vec3 N ) {
	float a = Roughness * Roughness;

	float Phi = TPI * Xi.x;
	float CosTheta = sqrt( (1.0 - Xi.y) / ( 1.0 + ( a*a - 1.0 ) * Xi.y ) );
	float SinTheta = sqrt( 1.0 - CosTheta * CosTheta );

	vec3 H;
	H.x = SinTheta * cos( Phi );
	H.y = SinTheta * sin( Phi );
	H.z = CosTheta;

	vec3 up = abs( N.x ) > EPS ? vec3( 0.0, 1.0, 0.0 ) : vec3( 1.0, 0.0, 0.0 );
	vec3 TangentX = normalize( cross( up, N ) );
	vec3 TangentY = cross( N, TangentX );
	// Tangent to world space
	return TangentX * H.x + TangentY * H.y + N * H.z;
}

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
			intersection.material = plane.material;
	
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
		intersection.position = ray.origin + ray.direction * t * 0.9;
		intersection.distance = t;
		intersection.normal = normalize( intersection.position - sphere.position );
		intersection.material = sphere.material;
		
	}
	
}

///  2 out, 3 in...
#define HASHSCALE3 vec3(.1031, .1030, .0973)
vec2 hash23(vec3 p3)
{
	p3 = fract(p3 * HASHSCALE3);
	p3 += dot(p3, p3.yzx+19.19);
	return fract((p3.xx+p3.yz)*p3.zy);
}


void shootRay( inout Intersection intersection, inout Ray ray, int bounce ) {

	intersection.hit = false;
	intersection.distance = INF;

	Material mat;
	mat.emmission = vec3( 0.0 );
	mat.roughness = 1.0;

	Plane plane;
	plane.position = vec3( 0, 0, 0 );
	plane.normal = vec3( 0, 1, 0 );
	plane.material = mat;
	plane.material.roughness = 0.2;
	plane.material.color = vec3( 0.8 );

	intersectionPlane( intersection, ray, plane );

	Sphere sphere;
	sphere.radius = 0.5;
	sphere.position = vec3( 0, 0.5, 0 );
	sphere.material = mat;
	sphere.material.color = vec3( 1.0, 1.0, 1.0 );
	sphere.material.roughness = 1.0;

	intersectionSphere( intersection, ray, sphere );

	if( intersection.hit ) {

			vec3 seed = vec3( gl_FragCoord.xy, float( time ) * 0.3 ) + float( bounce ) * 500.0 + 50.0;
			vec2 Xi = hash23( seed );

			vec3 H = importanceSampleGGX( Xi, intersection.material.roughness, intersection.normal );
			ray.direction = reflect( ray.direction, H );
			// ray.direction = reflect( ray.direction, intersection.normal );
			ray.origin = intersection.position;
			// ray.direction = intersection.normal;
		
	} else {

		intersection.material.emmission = vec3( 1.0, 1.0, 1.0 );
		intersection.material.color = vec3( 1.0 );

	}

}

vec3 radiance( inout Ray ray ) {

	Intersection intersection;

	vec3 acc = vec3( 0.0 );
	vec3 ref = vec3( 1.0 );

	for ( int i = 0; i < 20; i++ ) {

		shootRay( intersection, ray, i );

		vec3 emmission = intersection.material.emmission;
		vec3 color = intersection.material.color;
		acc += ref * emmission;
		ref *= color;

		if( !intersection.hit ) {

			break;
			
		}

	}

	return acc;
	// return intersection.hit ? intersection.normal : vec3( 0.0 );
	
}

void main( void ) {
	
	vec4 befTex = texture2D( backBuffer, vUv ) * min( frame, 1.0 ) ;

	Ray ray;
	ray.origin = cameraPosition;
	ray.direction = ( cameraMatrixWorld * cameraProjectionMatrixInverse * vec4( vUv * 2.0 - 1.0, 1.0, 1.0 ) ).xyz;
	ray.direction = normalize( ray.direction );

	vec4 o = vec4( ( befTex.xyz + radiance( ray ) ) , 1.0 );
	gl_FragColor = o;

}