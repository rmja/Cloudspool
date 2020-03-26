import {NewTemplateController, TemplateController, TemplatesController} from './templates.ctrl';

import {routing} from './templates.config';

export default angular.module('app.templates', [])
	.config(routing)
	.controller('TemplatesController', [
		'$scope',
		'TemplateResource',
		TemplatesController
	])
	.controller('NewTemplateController', [
		'$routeParams',
		'TemplateResource',
		NewTemplateController
	])
	.controller('TemplateController', [
		'template',
		'script',
		'TemplateResource',
		TemplateController
	])
	.name;