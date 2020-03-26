import { Template } from '../../../api/templates';
import {TemplateResource} from '../../../resources';

export class TemplateController {
	private template: TemplateViewModel;
	private originalScript: string;
	templateSuccess: string;
	templateError: string;

	get templateName() {
		return this.template.name;
	}

	get showTemplateSuccess() {
		return this.templateSuccess != null;
	}

	get showTemplateError() {
		return this.templateError != null;
	}

	get anyPendingChanges() {
		return this.originalScript != this.template.script;
	}

	get aceOptions() {
		var options: any = {};
		options.useSoftTabs = true;
		options.tabSize = 4;
		options.showInvisibles = true;
		options.mode = 'javascript';

		return options;
	}

	constructor(template: Template, script: string, private TemplateResource: TemplateResource) {
		this.template = {
			id: template.id,
			name: template.name,
			scriptUrl: template.scriptUrl,
			scriptContentType: template.scriptContentType,
			script: script,
		};
		this.originalScript = script;
	}

	save() {
		this.originalScript = this.template.script;

		this.templateSuccess = null;
		this.templateError = null;
		this.TemplateResource.setScript(this.template.id, this.template.script).then(() => {
			this.templateSuccess = 'Template saved successfully.';
		}).catch((error) => {
			debugger;
			this.templateError = error;
		});
	}
}

interface TemplateViewModel extends Template {
	script: string;
}