import * as ORE from '@ore-three-ts';
import { MainScene } from './MainScene';

class APP {

	private canvas: any;
	private controller: ORE.Controller;
	private scene: MainScene;

	constructor() {

		this.canvas = document.querySelector( "#canvas" );

		this.controller = new ORE.Controller( {

			canvas: this.canvas,
			retina: false,

		} );

		this.controller.bindScene( new MainScene() );

	}

}

window.addEventListener( 'load', ()=>{

	let app = new APP();

} );
