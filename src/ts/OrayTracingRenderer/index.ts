import * as THREE from 'three';
import * as ORE from '@ore-three-ts';

import pathTracingFrag from './shaders/path-tracing.fs';
import screenFrag from './shaders/screen.fs';
import { OrayTracingMaterial } from '../OrayTracingMaterial';

declare interface OrayRenderTargets {
	albedo: ORE.GPUcomputationData;
	material: ORE.GPUcomputationData;
	normal: ORE.GPUcomputationData;
	depth: ORE.GPUcomputationData;
}

export class OrayTracingRenderer extends ORE.GPUComputationController {

	private commonUniforms: ORE.Uniforms;

	private orayRenderTargets: OrayRenderTargets;

	private renderKernel: ORE.GPUComputationKernel;
	private renderResultData: ORE.GPUcomputationData;

	private renderScene: THREE.Scene;
	private screen: ORE.Background;

	constructor( renderer: THREE.WebGLRenderer, resolution: THREE.Vector2, parentUniforms?: ORE.Uniforms ) {

		let res = resolution.clone();

		super( renderer, res );

		this.commonUniforms = ORE.UniformsLib.CopyUniforms( {
			backBuffer: {
				value: null
			},
			albedoBuffer: {
				value: null
			},
			materialBuffer: {
				value: null
			},
			normalBuffer: {
				value: null
			},
			depthBuffer: {
				value: null
			},
			renderResult: {
				value: null
			},
			cameraMatrixWorld: {
				value: null
			},
			cameraProjectionMatrixInverse: {
				value: null
			},
			cameraProjectionMatrix: {
				value: null
			},
		}, parentUniforms );

		this.init();

	}

	public init() {

		this.renderKernel = this.createKernel( pathTracingFrag, this.commonUniforms );

		this.renderResultData = this.createData();

		this.orayRenderTargets = {
			albedo: this.createData( {
				depthBuffer: true
			} ),
			material: this.createData( {
				depthBuffer: true
			} ),
			normal: this.createData( {
				depthBuffer: true
			} ),
			depth: this.createData( {
				depthBuffer: true
			} ),
		};


		this.renderScene = new THREE.Scene();

		this.screen = new ORE.Background( {
			fragmentShader: screenFrag,
			uniforms: this.commonUniforms
		} );

		this.renderScene.add( this.screen );

	}
	public render( scene: THREE.Scene, camera: THREE.PerspectiveCamera, updateScene: boolean ) {

		let renderTargetMem = this.renderer.getRenderTarget();

		if ( updateScene ) {

			let keys = Object.keys( this.orayRenderTargets );

			for ( let i = 0; i < keys.length; i ++ ) {

				scene.traverse( ( obj ) => {

					if ( ( obj as THREE.Mesh ).isMesh && ( ( obj as THREE.Mesh ).material as OrayTracingMaterial ).isOrayTracingMaterial ) {

						( ( obj as THREE.Mesh ).material as OrayTracingMaterial ).setRenderType( i );

					}

				} );

				this.renderer.setRenderTarget( this.orayRenderTargets[ keys[ i ] ].buffer );

				this.renderer.render( scene, camera );

				this.commonUniforms[ keys[ i ] + 'Buffer' ].value = this.orayRenderTargets[ keys[ i ] ].buffer.texture;

			}

		}

		this.renderer.setRenderTarget( renderTargetMem );

		this.commonUniforms.backBuffer.value = this.renderResultData.buffer.texture;
		this.commonUniforms.cameraMatrixWorld.value = camera.matrixWorld.clone();
		this.commonUniforms.cameraProjectionMatrix.value = camera.projectionMatrix.clone();
		this.commonUniforms.cameraProjectionMatrixInverse.value = camera.projectionMatrixInverse.clone();

		this.compute( this.renderKernel, this.renderResultData, camera );

		this.commonUniforms.renderResult.value = this.renderResultData.buffer.texture;

		this.renderer.render( this.renderScene, camera );

	}

	public resize( resizeArgs: ORE.ResizeArgs ) {

		this.uniforms.dataSize.value.copy( resizeArgs.windowPixelSize );

		this.renderResultData.buffer.setSize( resizeArgs.windowPixelSize.x, resizeArgs.windowPixelSize.y );

	}

}
