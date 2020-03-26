import * as api from "../api/spoolers";

import { Operation } from "ur-jsonpatch";
import { ResourceCache } from './cache';

export class SpoolerResource {
	public cache: ResourceCache<api.Spooler, number>;

	constructor($rootScope: ng.IRootScopeService) {
		this.cache = new ResourceCache<api.Spooler, number>($rootScope, x => x.id);
	}

	public getAllByZoneId(zoneId: number) {
		return api.getAllByZoneId(zoneId).transfer().then(x => this.cache.ensureAll(x));
	}

	public getById(id: number) {
		return api.getById(id).transfer().then(x => this.cache.ensure(x));
	}

	public create(command: { zoneId: number, name: string }) {
		return api.create(command).transfer().then(x => this.cache.add(x));
	}

	public update(id: number, patch: Operation[]) {
		return api.update(id, patch).transfer().then(x => this.cache.ensure(x));
	}

	public destroy(id: number) {
		return api.destroy(id).send().then(() => this.cache.remove(id));
	}
}