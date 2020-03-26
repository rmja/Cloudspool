import * as api from "../api/projects";

import { IRootScopeService } from 'angular';
import { ResourceCache } from './cache';

export class ProjectResource {
	public cache: ResourceCache<api.Project, number>;

	constructor($rootScope: IRootScopeService) {
		this.cache = new ResourceCache<api.Project, number>($rootScope, x => x.id);
	}

	public getByKey(key: string) {
		return api.getByKey(key).transfer().then(project => this.cache.ensure(project));
	}
}