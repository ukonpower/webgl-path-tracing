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

		this.initParam();

		let gltfLoader = new GLTFLoader();

		gltfLoader.load( './assets/webgl-path-tracing.glb', ( gltf ) => {

			this.scene.add( gltf.scene );

			this.initScene();

		} );


	}

	public initParam() {

		this.param = {
			metalness: 0,
			albedo: '#F00',
			roughness: 0.1
		};

		// this.pane = new Tweakpane();

		// this.pane.addInput( this.param, 'metalness', {
		// 	min: 0, max: 1
		// } );

		// this.pane.addInput( this.param, 'roughness', {
		// 	min: 0, max: 1
		// } );

		// this.pane.addInput( this.param, 'albedo' );

	}

	private updatePane() {

		// this.commonUniforms.metalness.value = this.param.metalness;
		// this.commonUniforms.roughness.value = this.param.roughness;
		// this.commonUniforms.albedo.value.set( this.param.albedo );

	}

	public initScene() {

		// this.camera.position.set( 0, 2, 5 );
		// this.controls.target = new THREE.Vector3( - 0.2, 0.3, 0 );

		this.camera.position.set( - 2, 3, 6 );
		this.controls.target = new THREE.Vector3( - 0.2, 0.3, 0 );

		let mat: OrayTracingMaterial;
		let geo: THREE.BufferGeometry;
		let mesh: THREE.Mesh;

		( this.scene.getObjectByName( 'Plane' ) as THREE.Mesh ).material = new OrayTracingMaterial( {
			roughness: 0.0,
			metalness: 0.1
		} );

		( this.scene.getObjectByName( 'Suzanne_0' ) as THREE.Mesh ).material = new OrayTracingMaterial( {
			albedo: new THREE.Vector3( 1, 0, 0 ),
			roughness: 0.3,
			metalness: 0.0,
		} );

		( this.scene.getObjectByName( 'Suzanne_1' ) as THREE.Mesh ).material = new OrayTracingMaterial( {
			albedo: new THREE.Vector3( 0, 1, 0 ),
			roughness: 0.6,
			metalness: 0.5
		} );

		( this.scene.getObjectByName( 'Cube' ) as THREE.Mesh ).material = new OrayTracingMaterial( {
			albedo: new THREE.Vector3( 0, 0, 1 ),
			roughness: 1.0,
			metalness: 0.5,
		} );

		( this.scene.getObjectByName( 'Icosphere' ) as THREE.Mesh ).material = new OrayTracingMaterial( {
			albedo: new THREE.Vector3( 1, 0, 1 ),
			roughness: 1.0,
			metalness: 0.5
		} );

		( this.scene.getObjectByName( 'Torus' ) as THREE.Mesh ).material = new OrayTracingMaterial( {
			albedo: new THREE.Vector3( 0, 1, 0 ),
			roughness: 1.0,
			metalness: 1.0
		} );

	}

	public animate( deltaTime: number ) {

		this.commonUniforms.time.value = this.time;

		this.updatePane();

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
