import {IZone} from '../../models';

export class DashboardController {
	get showWelcome() {
		return this.zones.length == 0;
	}

	constructor(private zones: Array<IZone>) {
	}
}