import * as api from "../api/terminals";

import { IRootScopeService } from 'angular';
import { Operation } from "ur-jsonpatch";
import { ResourceCache } from './cache';

export class TerminalResource {
	public cache: ResourceCache<api.Terminal, number>;

	constructor($rootScope: IRootScopeService) {
		this.cache = new ResourceCache<api.Terminal, number>($rootScope, x => x.id);
	}

	public getAllByZoneId(zoneId: number) {
		return api.getAllByZoneId(zoneId).transfer().then(terminals => this.cache.ensureAll(terminals));
	}

	public getById(id: number) {
		return api.getById(id).transfer().then(terminal => this.cache.ensure(terminal));
	}

	public create(command: { zoneId: number, name: string, routes?: { alias: string, spoolerId: number, printerName: string }[] }) {
		return api.create(command).transfer().then(terminal => this.cache.ensure(terminal));
	}

	public update(id: number, patch: Operation[]) {
		return api.update(id, patch).transfer().then(terminal => this.cache.ensure(terminal));
	}

	public destroy(id: number) {
		return api.destroy(id).send().then(() => this.cache.remove(id));
	}
}