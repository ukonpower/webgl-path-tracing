import * as ORE from '@ore-three-ts';
import * as THREE from 'three';
import { PathTracingRenderer } from './PathTracingRenderer';

export class MainScene extends ORE.BaseScene {

	private commonUniforms: ORE.Uniforms;
	private pathTracingRenderer: PathTracingRenderer;

	constructor() {

		super();

		this.name = "MainScene";

		this.commonUniforms = {
			time: {
				value: 0
			}
		};

	}

	onBind( gProps: ORE.GlobalProperties ) {

		super.onBind( gProps );

		this.renderer = this.gProps.renderer;

		this.pathTracingRenderer = new PathTracingRenderer( this.renderer, this.gProps.resizeArgs.windowPixelSize, this.commonUniforms );

		this.initScene();

	}

	public initScene() {

		this.camera.position.set( 0, 2, 5 );
		this.camera.lookAt( 0, 0, 0 );

		this.camera.matrixWorldNeedsUpdate = true;

	}

	public animate( deltaTime: number ) {

		this.commonUniforms.time.value = this.time;

		this.pathTracingRenderer.render( this.camera );

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
