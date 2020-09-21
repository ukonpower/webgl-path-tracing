import * as THREE from 'three';
import * as ORE from '@ore-three-ts';

import orayVert from './shaders/oray.vs';
import orayFrag from './shaders/oray.fs';

export declare interface OrayShaderMaterialParam extends THREE.ShaderMaterialParameters {
	baseMaterial?: THREE.MeshStandardMaterial;
	albedo?: THREE.Vector3;
	emission?: THREE.Vector3
	roughness?: number;
	metalness?: number;
}

export class OrayTracingMaterial extends THREE.ShaderMaterial {

	constructor( param?: OrayShaderMaterialParam ) {

		param = param || {};

		param.fragmentShader = param.fragmentShader || orayFrag;
		param.vertexShader = param.vertexShader || orayVert;

		param.uniforms = param.uniforms || {};

		if ( param.uniforms.renderType ) {

			console.warn( '"renderType" uniform cannnot be used.' );

		}

		param.uniforms.renderType = { value: 0 };

		let albedoMap = param.baseMaterial && param.baseMaterial.map;
		let roughnessMap = param.baseMaterial && param.baseMaterial.roughnessMap;
		let metalnessMap = param.baseMaterial && param.baseMaterial.metalnessMap;
		let normalMap = param.baseMaterial && param.baseMaterial.normalMap;

		param.defines = {
			"USE_ALBEDOMAP": ( albedoMap != null ) && param.albedo == null,
			"USE_ROUGHNESSMAP": ( roughnessMap != null ) && param.roughness == null,
			"USE_METALNESSMAP": ( metalnessMap != null ) && param.metalness == null,
			"USE_NORMALMAP": ( normalMap != null ),
		};

		param.uniforms.albedo = param.uniforms.albedo || { value: param.albedo || new THREE.Vector3( 1, 1, 1 ) };
		param.uniforms.emission = param.uniforms.emission || { value: param.emission || new THREE.Vector3() };
		param.uniforms.roughness = param.uniforms.roughness || { value: param.roughness != null ? param.roughness : 0.5 };
		param.uniforms.metalness = param.uniforms.metalness || { value: param.metalness != null ? param.metalness : 0.0 };

		param.uniforms.albedoMap = { value: albedoMap };
		param.uniforms.roughnessMap = { value: roughnessMap };
		param.uniforms.metalnessMap = { value: metalnessMap };
		param.uniforms.normalMap = { value: normalMap };

		param.extensions = {
			derivatives: true
		}

		super( param );

	}

	public set albedo( value: THREE.Vector3 ) {

		this.uniforms.albedo.value.copy( value );

	}

	public set emission( value: THREE.Vector3 ) {

		this.uniforms.emission.value = value;

	}

	public set roughness( value: number ) {

		this.uniforms.roughness.value = value;

	}

	public set metalness( value: number ) {

		this.uniforms.metalness.value = value;

	}

	/**
	 *出力するマテリアルのタイプを指定します。
	 *
	 * @param {number} type 0=albedo 1=material 2=normal 3=depth
	 * @memberof OrayTracingMaterial
	 */
	public setRenderType( type: number ) {

		this.uniforms.renderType.value = type;

		this.side = type >= 5 ? THREE.BackSide : THREE.FrontSide;

	}

	public get isOrayTracingMaterial() {

		return true;

	}

}
