import {FormatResource, ResourceResource, TemplateResource} from '../../../resources';

import { Format } from '../../../api/formats';
import { Template } from '../../../api/templates';

export class FormatsController {
	zoneId: number;
	formats: Array<Format>;

	constructor($scope: ng.IScope, $routeParams,
		private FormatResource: FormatResource,
		private TemplateResource: TemplateResource,
		private ResourceResource: ResourceResource) {
		this.zoneId = parseInt($routeParams.zoneId);

		this.formats = FormatResource.cache.getAll(x => x.zoneId === this.zoneId);
		$scope.$watch(FormatResource.cache.getLastModified,() => { this.formats = FormatResource.cache.getAll(x => x.zoneId === this.zoneId); });
	}

	saveFormat(format: Format) {
		this.FormatResource.set(format);
	}

	getTemplateName(format: Format) {
		var template = this.TemplateResource.cache.get(format.templateId);
		return template.name;
	}

	destroy(format: Format) {
		this.FormatResource.destroy(format.zoneId, format.alias);
	}
}

export class NewFormatController {
	zoneId: number;
	templates: Array<Template>;
	selectedTemplate: Template;
	alias: string;

	errorMessage: string;

	get showError() {
		return this.errorMessage != null;
	}

	constructor($routeParams,
		private FormatResource: FormatResource,
		private TemplateResource: TemplateResource) {
		this.zoneId = parseInt($routeParams.zoneId);

		this.templates = TemplateResource.cache.getAll()
		this.selectedTemplate = _.first(this.templates);
	}

	submit() {
		this.errorMessage = null;
		this.FormatResource.set({
			zoneId: this.zoneId,
			templateId: this.selectedTemplate.id,
			alias: this.alias
		}).catch((error) => {
			debugger;
			this.errorMessage = error.data;
		});
	}
}