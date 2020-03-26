import { Resource } from '../../../api/resources';
import { ResourceResource } from '../../../resources';

export class ResourceController {
	private originalContent: string;
	private resourceContent: string;
	resourceSuccess: string;
	resourceError: string;

	get resourceAlias() {
		return this.resource.alias;
	}

	get showResourceSuccess() {
		return this.resourceSuccess != null;
	}

	get showResourceError() {
		return this.resourceError != null;
	}

	get anyPendingChanges() {
		return this.originalContent != this.resourceContent;
	}

	constructor(private resource: Resource, resourceContent: unknown, private ResourceResource: ResourceResource, public jsonAceOptions) {
		this.originalContent = this.resourceContent = JSON.stringify(resourceContent);
	}

	save() {
		this.originalContent = this.resourceContent;

		this.ResourceResource.setContent(this.resource.alias, this.resourceContent, this.resource.contentType).then(() => {
			this.resourceSuccess = 'Resource saved successfully.';
			this.resourceError = null;

		}).catch((error) => {
			this.resourceSuccess = null;
			this.resourceError = error;
		});
	}
}