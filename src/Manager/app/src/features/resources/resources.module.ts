import {NewResourceObjectController, NewResourceUploadController, ResourceController, ResourcesController} from './resources.ctrl';

import {routing} from './resources.config';

export default angular.module('app.resources', [])
	.config(routing)
	.controller('ResourcesController', [
		'$scope',
		'ResourceResource',
		'ProjectService',
		ResourcesController
	])
	.controller('NewResourceObjectController', [
		'ResourceResource',
		NewResourceObjectController
	])
	.controller('NewResourceUploadController', [
		'ResourceResource',
		NewResourceUploadController
	])
	.controller('ResourceController', [
		'resource',
		'resourceContent',
		'ResourceResource',
		'jsonAceOptions',
		ResourceController
	])
	.name;