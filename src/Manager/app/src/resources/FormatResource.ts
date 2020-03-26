import * as api from "../api/formats";

import { IRootScopeService } from 'angular';
import { ResourceCache } from './cache';

export class FormatResource {
	public cache: ResourceCache<api.Format, string>;

	constructor($rootScope: IRootScopeService) {
		this.cache = new ResourceCache<api.Format, string>($rootScope, x => `${x.zoneId}~${x.alias}`);
	}

	public getAll(zoneId: number) {
		return api.getAllByZone(zoneId).transfer().then(formats => this.cache.ensureAll(formats));
	}

	public set(command: api.Format) {
		return api.set(command).transfer().then(format => this.cache.ensure(format));
	}

	public destroy(zoneId: number, alias: string) {
		return api.destroy(zoneId, alias).send().then(() => this.cache.remove(`${zoneId}~${alias}`));
	}
}