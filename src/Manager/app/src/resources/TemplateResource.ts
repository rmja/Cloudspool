import * as api from "../api/templates";

import { IRootScopeService } from 'angular';
import { Operation } from "ur-jsonpatch";
import { ResourceCache } from './cache';

export class TemplateResource {
	public cache: ResourceCache<api.Template, number>;

	constructor($rootScope : IRootScopeService) {
		this.cache = new ResourceCache<api.Template, number>($rootScope, x => x.id);
	}

	public getAll() {
		return api.getAll().transfer().then(templates => this.cache.setAll(templates));
	}

	public getById(id: number) {
		return api.getById(id).transfer().then(template => this.cache.ensure(template));
	}

	public getScriptById(templateId: number) {
		return api.getScriptById(templateId).transfer();
	}

	public create(command: { name: string, script: string, scriptContentType }) {
		return api.create(command).transfer().then(template => this.cache.add(template));
	}

	public update(id: number, patch: Operation[]) {
		return api.update(id, patch).transfer().then(updated => this.cache.ensure(updated));
	}

	public setScript(templateId: number, script: string, scriptContentType: string) {
		return api.setScript(templateId, script, scriptContentType).send();
	}

	public destroy(templateId: number) {
		return api.destroy(templateId).send().then(() => this.cache.remove(templateId));
	}
}