import * as api from "../api/resources";

import { FileReaderService } from '../services';
import { ResourceCache } from './cache';

const jsonContentType = 'application/json';

export class ResourceResource {
	public cache: ResourceCache<api.Resource, string>;

	private extensionContentTypeMap = {
		'.bmp': 'image/bmp',
		'.json': jsonContentType
	}

	constructor($rootScope: ng.IRootScopeService, private FileReaderService: FileReaderService) {
		this.cache = new ResourceCache<api.Resource, string>($rootScope, x => x.alias);
	}

	public getAll() {
		return api.getAll().transfer().then(x => this.cache.setAll(x));
	}

	public getByAlias(alias: string) {
		return api.getByAlias(alias).transfer().then(x => this.cache.ensure(x));
	}

	public getJsonContent(alias: string) {
		return api.getContentByAlias(alias).expectJson().transfer();
	}

	public setContent(alias: string, content: string | Uint8Array, contentType: string) {
		return api.set(alias, content, contentType).transfer().then(x => this.cache.ensure(x));
	}

	public setContentFile(alias: string, file: File) {
		var contentType = file.type;

		switch (contentType) {
			case 'image/bmp':
				break;
			default:
				var extensions = _.keys(this.extensionContentTypeMap);
				var extension = _.find(extensions, (extension) => {
					return _.str.endsWith(file.name, extension);
				});

				if (extension) {
					contentType = this.extensionContentTypeMap[extension];
				}
				else {
					throw new Error('Content type cannot be derived from the file extension. Supported extensions are: ' + extensions.join(', ') + '.');
				}
				break;
		}

		switch (contentType) {
			case 'image/bmp':
				return this.FileReaderService.readBinary(file).then(content => {
					return this.setContent(alias, content, contentType);
				});
			case jsonContentType:
				return this.FileReaderService.readText(file).then(content => {
					return this.setContent(alias, content, contentType);
				});
		}

		throw new Error("Unsupported content type");
	}

	public destroy(alias: string) {
		return api.destroy(alias).send().then(() => this.cache.remove(alias));
	}
}