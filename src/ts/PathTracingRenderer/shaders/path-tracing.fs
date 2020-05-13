uniform vec2 dataSize;
uniform mat4 cameraMatrixWorld;
uniform mat4 cameraProjectionMatrixInverse;

uniform float time;
uniform float frame;

uniform sampler2D backBuffer;

varying vec2 vUv;

#define MAX_BOUNCE 2

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
	vec3 diffseColor;
	vec3 specularColor;
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

//法線分布関数
float GGX( float nh, float a ) { 

	a = max( 0.005, a );

	float a2 = a * a;
	float nh2 = nh * nh;
	float d = nh2 * ( a2 - 1.0 ) + 1.0;

	return a2 / ( PI * d * d );
	
}

//幾何減衰項
float SmithSchlickGGX( float NV, float NL, float a ) {

	float k = ( a ) / 2.0;

	float v = NV / ( NV * ( 1.0 - k ) + k + 0.0001 );
	float l = NL / ( NL * ( 1.0 - k ) + k + 0.0001 );

	return v * l;

}

//フレネル
vec3 Schlick( vec3 f0, float HV ) {

	return f0 + ( 1.0 - f0 ) * pow( 1.0 - HV, 5.0 );

}


vec3 random3D( vec2 p, float seed ) {

	return vec3(
		random( p + seed ),
		random( p + seed + 100.0 ),
		random( p + seed + 303.2)
	);
	
}

void reflection( Intersection intersection, inout Ray ray, int bounce ) {

	float seed =  frame * 0.001 + float( bounce );

	vec2 rnd = vec2( random( vUv + sin( seed ) ), random( vUv - cos( seed) ) );
	vec3 normal = intersection.normal;
	
	//diffuse
	float r = sqrt( rnd.x );
	float theta = TPI * rnd.y;

	vec3 tDir = vec3( r * cos( theta ), r * sin( theta ), sqrt( 1.0 - rnd.x ) );

	vec3 tangent = normalize( cross( normal, abs( normal.x ) > EPS ? vec3( 0.0, 1.0, 0.0 ) : vec3( 1.0, 0.0, 0.0 ) ) );
	vec3 binormal = cross( tangent, normal );
	
	ray.direction = tangent * tDir.x + binormal * tDir.y + normal * tDir.z;
	ray.origin = intersection.position;

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

	vec3 oc = ray.origin - sphere.position;
    float a = dot( ray.direction, ray.direction );
    float b = 2.0 * dot( oc, ray.direction );
    float c = dot( oc,oc ) - sphere.radius * sphere.radius;
    float discriminant = b * b - 4.0 * a * c;
	float t = ( -b - sqrt( discriminant ) ) / ( 2.0 * a );

	if( discriminant > 0.0 && t > EPS && t < intersection.distance ) {

		intersection.hit = true;
		intersection.position = ray.origin + ray.direction * t;
		intersection.distance = t;
		intersection.normal = normalize( intersection.position - sphere.position );
		intersection.material = sphere.material;
		
	}
	
}

void shootRay( inout Intersection intersection, inout Ray ray, int bounce ) {

	intersection.hit = false;
	intersection.distance = INF;


	Plane plane;
	plane.position = vec3( 0, 0, 0 );
	plane.normal = normalize( vec3( 0.0, 1, 0 ) );
	plane.material.roughness = 0.9;
	plane.material.albedo = vec3( 0.8 );
	intersectionPlane( intersection, ray, plane );

	Sphere sphere;
	sphere.radius = 0.5;
	sphere.position = vec3( -0.5, 0.5, 0 );
	sphere.material.albedo = vec3( 1.0, 0.0, 0.0 );
	sphere.material.roughness = 0.8;
	intersectionSphere( intersection, ray, sphere );

	sphere.radius = 0.5;
	sphere.position = vec3( 0.6, 0.5, 0 );
	sphere.material.albedo = vec3( 1.0, 1.0, 1.0 );
	sphere.material.roughness = 0.8;
	intersectionSphere( intersection, ray, sphere );

	//light
	sphere.radius = 2.0;
	sphere.position = vec3( 0.0, 5.0, 0.0 );
	sphere.material.roughness = 1.0;
	sphere.material.emmission = vec3( 1.0 );
	// intersectionSphere( intersection, ray, sphere );

	if( intersection.hit ) {

		reflection( intersection, ray, bounce );

		
	} else {

		intersection.material.emmission = vec3( 1.0 );

	}

}

vec3 radiance( inout Ray ray ) {

	Intersection intersection;

	vec3 acc = vec3( 0.0 );
	vec3 ref = vec3( 1.0 );

	for ( int i = 0; i < 20; i++ ) {

		shootRay( intersection, ray, i );

		vec3 emmission = intersection.material.emmission;
		vec3 color = intersection.material.albedo;
		acc += ref * emmission;
		ref *= color;

		if( !intersection.hit ) {

			break;
			
		}

		// acc = ray.direction;
		// break;

	}

	return acc;
	
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