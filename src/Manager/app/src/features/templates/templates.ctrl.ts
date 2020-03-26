import { Template } from '../../api/templates';
import { TemplateResource } from '../../resources';

export { TemplateController } from './template/template.ctrl';

export class TemplatesController {
	templates: Array<Template>;

	constructor($scope: ng.IScope,
		private TemplateResource: TemplateResource) {
		this.templates = TemplateResource.cache.getAll();

		$scope.$watch(TemplateResource.cache.getLastModified, () => {
			this.templates = TemplateResource.cache.getAll();
		});
	}

	saveTemplate(template: Template) {
		this.TemplateResource.update(template.id, [
			{ op: "replace", path: "/name", value: template.name }
		]);
	}

	destroy(template: Template) {
		this.TemplateResource.destroy(template.id);
	}
}

export class NewTemplateController {
	name: string;

	constructor($routeParams,
		private TemplateResource: TemplateResource) {
	}

	submit() {
		this.TemplateResource.create({
			name: this.name,
		});
	}
}