import { Zone } from '../../../api/zones';

export * from './formats.ctrl';
export * from './terminals.ctrl';
export * from './spoolers.ctrl';
export * from './routes.ctrl';

export class ZoneController {

	get zoneName() {
		return this.zone.name;
	}

	constructor(private zone: Zone) {
	}
}