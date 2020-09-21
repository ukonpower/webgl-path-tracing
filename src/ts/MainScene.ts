import * as ORE from '@ore-three-ts';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
import { OrayTracingRenderer } from './OrayTracingRenderer';

import Tweakpane from 'tweakpane';
import { OrayTracingMaterial } from './OrayTracingMaterial';

import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader';

export class MainScene extends ORE.BaseScene {

	private commonUniforms: ORE.Uniforms;
	private pathTracingRenderer: OrayTracingRenderer;
	private controls: OrbitControls;

	private pane: Tweakpane;
	private param: any;

	constructor() {

		super();

		this.name = "MainScene";

		this.commonUniforms = {
			time: {
				value: 0
			},
			frame: {
				value: 0
			},
		};

	}

	onBind( gProps: ORE.GlobalProperties ) {

		super.onBind( gProps );

		this.renderer = this.gProps.renderer;

		this.controls = new OrbitControls( this.camera, this.renderer.domElement );

		this.controls.addEventListener( 'change', () => {

			this.commonUniforms.frame.value = 0.0;

		} );

		this.pathTracingRenderer = new OrayTracingRenderer( this.renderer, this.gProps.resizeArgs.windowPixelSize.multiplyScalar( 1.0 ), this.commonUniforms );

		let gltfLoader = new GLTFLoader();

		gltfLoader.load( './assets/webgl-path-tracing.glb', ( gltf ) => {

			this.scene.add( gltf.scene );

			this.initScene();

		} );

		let loader = new THREE.CubeTextureLoader();
		loader.load( [
			'./assets/img/cubemap/Bridge2/posx.jpg',
			'./assets/img/cubemap/Bridge2/negx.jpg',
			'./assets/img/cubemap/Bridge2/posy.jpg',
			'./assets/img/cubemap/Bridge2/negy.jpg',
			'./assets/img/cubemap/Bridge2/posz.jpg',
			'./assets/img/cubemap/Bridge2/negz.jpg',
		], ( tex ) => {

			this.scene.background = tex;

		} );


	}

	public initScene() {

		this.camera.position.set( 4, 1, 4 );
		this.controls.target = new THREE.Vector3( - 0.2, 0.3, 0 );

		this.scene.traverse( ( obj: THREE.Mesh ) => {

			if ( obj.isMesh ) {

				obj.material = new OrayTracingMaterial( {
					baseMaterial: obj.material
				} );

				if ( obj.name.indexOf( 'Light' ) > - 1 ) {

					( obj.material as OrayTracingMaterial ).emission = new THREE.Vector3( 10, 10, 10 );

				}

			}

		} );

	}

	public animate( deltaTime: number ) {

		this.commonUniforms.time.value = this.time;

		this.controls.update();

		this.camera.updateMatrixWorld();

		this.pathTracingRenderer.render( this.scene, this.camera, this.commonUniforms.frame.value == 0 );

		this.commonUniforms.frame.value += 1.0;

	}

	public onResize( args: ORE.ResizeArgs ) {

		super.onResize( args );

	}

	public onTouchStart( cursor: ORE.Cursor, event: MouseEvent ) {

	}

	public onTouchMove( cursor: ORE.Cursor, event: MouseEvent ) {

		this.commonUniforms.frame.value = 0.0;

	}

	public onTouchEnd( cursor: ORE.Cursor, event: MouseEvent ) {

	}

	public onHover( cursor: ORE.Cursor ) {

	}

	public onWheel( event: WheelEvent, trackpadDelta: number ) {

	}

}
