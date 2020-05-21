import * as ORE from '@ore-three-ts';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
import { PathTracingRenderer } from './PathTracingRenderer';

import Tweakpane from 'tweakpane';

export class MainScene extends ORE.BaseScene {

	private commonUniforms: ORE.Uniforms;
	private pathTracingRenderer: PathTracingRenderer;
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
			roughness: {
				value: 0
			},
			metalness: {
				value: 0
			},
			albedo: {
				value: new THREE.Color()
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

		this.pathTracingRenderer = new PathTracingRenderer( this.renderer, this.gProps.resizeArgs.windowPixelSize, this.commonUniforms );

		this.initParam();

		this.initScene();

	}

	public initParam() {

		this.param = {
			metalness: 0,
			albedo: '#F00',
			roughness: 0.1
		};

		this.pane = new Tweakpane();

		this.pane.addInput( this.param, 'metalness', {
			min: 0, max: 1
		} );

		this.pane.addInput( this.param, 'roughness', {
			min: 0, max: 1
		} );

		this.pane.addInput( this.param, 'albedo' );

	}

	private updatePane() {

		// let keys = Object.keys( this.param );

		// for ( let i = 0; i < keys.length; i++ ) {

		// 	this.commonUniforms[ keys[i] ] && this.commonUniforms[ keys[i] ].value = this.param[keys[i]];

		// }

		this.commonUniforms.metalness.value = this.param.metalness;
		this.commonUniforms.roughness.value = this.param.roughness;
		this.commonUniforms.albedo.value.set( this.param.albedo );

	}

	public initScene() {

		this.camera.position.set( 0, 0.5, 3 );
		this.camera.lookAt( 0, 0.4, 0 );

	}

	public animate( deltaTime: number ) {

		this.commonUniforms.time.value = this.time;

		this.updatePane();

		this.controls.update();

		this.camera.updateMatrixWorld();

		this.pathTracingRenderer.render( this.camera );

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
