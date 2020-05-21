uniform vec2 dataSize;
uniform mat4 cameraMatrixWorld;
uniform mat4 cameraProjectionMatrixInverse;

uniform float roughness;
uniform float metalness;
uniform vec3 albedo;

uniform float time;
uniform float frame;

uniform sampler2D backBuffer;

varying vec2 vUv;

#define MAX_BOUNCE 10

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

int shootRay( inout Intersection intersection, inout Ray ray, int bounce ) {

	intersection.hit = false;
	intersection.distance = INF;

	Plane plane;
	plane.position = vec3( 0, 0, 0 );
	plane.normal = normalize( vec3( 0.0, 1, 0 ) );
	plane.material.roughness = 0.0;
	plane.material.albedo = vec3( 1.0 );
	plane.material.metalness = 0.0;
	intersectionPlane( intersection, ray, plane );

	Sphere redSphereMetal;
	redSphereMetal.radius = 0.5;
	redSphereMetal.position = vec3( -0.55, 0.5, 0.45 );
	redSphereMetal.material.albedo = vec3( 1.0, 0.0, 0.0 );
	redSphereMetal.material.metalness = 1.0;
	redSphereMetal.material.roughness = 0.2;
	intersectionSphere( intersection, ray, redSphereMetal );

	Sphere redSphere;
	redSphere.radius = 0.5;
	redSphere.position = vec3( 0.55, 0.5, 0.45 );
	redSphere.material.albedo = albedo;
	redSphere.material.metalness = metalness;
	redSphere.material.roughness = roughness;
	intersectionSphere( intersection, ray, redSphere );

	Sphere whiteSphere;
	whiteSphere.radius = 0.5;
	whiteSphere.position = vec3( 0.0, 0.5, -0.45 );
	whiteSphere.material.albedo = vec3( 1.0 );
	whiteSphere.material.metalness = 0.0;
	whiteSphere.material.roughness = 1.0;
	intersectionSphere( intersection, ray, whiteSphere );

	//light
	Sphere lightSphere;
	lightSphere.radius = 2.0;
	lightSphere.position = vec3( 0.0, 5.0, 0.0 );
	lightSphere.material.roughness = 1.0;
	lightSphere.material.emission = vec3( 10.0 );
	intersectionSphere( intersection, ray, lightSphere );

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
		intersection.material.emission = vec3( 0.0 );

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

	return col;
	
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