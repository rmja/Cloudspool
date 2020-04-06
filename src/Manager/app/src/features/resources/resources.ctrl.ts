import {ProjectService} from '../../services';
import { Resource } from '../../api/resources';
import {ResourceResource} from '../../resources';

export {ResourceController} from './resource/resource.ctrl';

export class ResourcesController {
	resources: Array<Resource>;

	constructor($scope: ng.IScope,
		private ResourceResource: ResourceResource,
		private ProjectService: ProjectService) {
		this.resources = ResourceResource.cache.getAll();

		$scope.$watch(ResourceResource.cache.getLastModified,() => {
			this.resources = ResourceResource.cache.getAll();
		});
	}

	contentUrl(resource: Resource) {
		return resource.contentUrl + '?project_key=' + this.ProjectService.projectKey;
	}

	isObjectResource(resource: Resource) {
		return resource.contentType.indexOf('application/json') === 0;
	}

	destroy(resource: Resource) {
		this.ResourceResource.destroy(resource.alias);
	}
}

export class NewResourceUploadController {
	alias: string;
	file: File;
	successMessage: string;
	errorMessage: string;

	get showSuccess() {
		return this.successMessage != null;
	}

	get showError() {
		return this.errorMessage != null;
	}

	constructor(private ResourceResource: ResourceResource) {
	}

	submit() {
		this.successMessage = null;
		this.errorMessage = null;
		this.ResourceResource.setContentFile(this.alias, this.file).then(() => {
			this.successMessage = 'Resource "' + this.alias + '" uploaded with success.';
		}).catch((reason) => {
			this.errorMessage = reason;
		});
	}
}

export class NewResourceObjectController {
	alias: string;
	errorMessage: string;
	
	get showError() {
		return this.errorMessage != null;
	}

	constructor(private ResourceResource: ResourceResource) {
	}

	submit() {
		this.errorMessage = null;
		this.ResourceResource.setContent(this.alias, '{}', 'application/json').catch((reason) => {
			this.errorMessage = reason;
		});
	}
}