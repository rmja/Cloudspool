import * as api from "../api/zones";

import { IRootScopeService } from 'angular';
import { Operation } from "ur-jsonpatch";
import { ResourceCache } from './cache';

export class ZoneResource {
	public cache: ResourceCache<api.Zone, number>;

	constructor($rootScope : IRootScopeService) {
		this.cache = new ResourceCache<api.Zone, number>($rootScope, x => x.id);
	}

	public getAll() {
		return api.getAll().transfer().then(x => this.cache.setAll(x));
	}

	public getById(id: number) {
		return api.getById(id).transfer().then(template => this.cache.ensure(template));
	}

	public create(command: { name: string, routes?: { alias: string, spoolerId: number, printerName: string }[] }) {
		return api.create(command).transfer().then(zone => this.cache.add(zone));
	}

	public update(id: number, patch: Operation[]) {
		return api.update(id, patch).transfer().then(updated => this.cache.ensure(updated));
	}

	public destroy(zoneId: number) {
		return api.destroy(zoneId).send().then(() => this.cache.remove(zoneId));
	}
}