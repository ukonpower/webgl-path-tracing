import * as ORE from '@ore-three-ts';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
import { PathTracingRenderer } from './PathTracingRenderer';

export class MainScene extends ORE.BaseScene {

	private commonUniforms: ORE.Uniforms;
	private pathTracingRenderer: PathTracingRenderer;
	private controls: OrbitControls;

	constructor() {

		super();

		this.name = "MainScene";

		this.commonUniforms = {
			time: {
				value: 0
			},
			frame: {
				value: 0
			}
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

		this.initScene();

	}

	public initScene() {

		this.camera.position.set( 0, 0.5, 3 );
		this.camera.lookAt( 0, 0.4, 0 );

	}

	public animate( deltaTime: number ) {

		this.commonUniforms.time.value = this.time;

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

	}

	public onTouchEnd( cursor: ORE.Cursor, event: MouseEvent ) {

	}

	public onHover( cursor: ORE.Cursor ) {

	}

	public onWheel( event: WheelEvent, trackpadDelta: number ) {

	}

}
