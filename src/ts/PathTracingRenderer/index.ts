import * as THREE from 'three';
import * as ORE from '@ore-three-ts';

import pathTracingFrag from './shaders/path-tracing.fs';
import screenFrag from './shaders/screen.fs';

export class PathTracingRenderer extends ORE.GPUComputationController {

	private commonUniforms: ORE.Uniforms;

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
			renderResult: {
				value: null
			},
			cameraMatrixWorld: {
				value: null
			},
			cameraProjectionMatrixInverse: {
				value: null
			},
		}, parentUniforms );

		this.init();

	}

	public init() {

		this.renderKernel = this.createKernel( pathTracingFrag, this.commonUniforms );

		this.renderResultData = this.createData();

		this.renderScene = new THREE.Scene();

		this.screen = new ORE.Background( {
			fragmentShader: screenFrag,
			uniforms: this.commonUniforms
		} );

		this.renderScene.add( this.screen );

	}

	public render( camera: THREE.PerspectiveCamera ) {

		this.commonUniforms.backBuffer.value = this.renderResultData.buffer.texture;
		this.commonUniforms.cameraMatrixWorld.value = camera.matrixWorld.clone();
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
