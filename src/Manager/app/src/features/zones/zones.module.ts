import {
	FormatsController,
	NewFormatController,
	NewSpoolerController,
	NewTerminalController,
	NewTerminalRouteController,
	NewZoneRouteController,
	RoutesController,
	SpoolersController,
	TerminalsController,
	ZoneController
} from './zone/zone.ctrl';
import {NewZoneController, ZonesController} from './zones.ctrl';

import {routing} from './zones.config';

export default angular.module('app.zones', [])
	.config(routing)
	.controller('ZonesController', [
		'$scope',
		'ZoneResource',
		ZonesController
	])
	.controller('NewZoneController', [
		'ZoneResource',
		NewZoneController
	])
	.controller('ZoneController', [
		'zone',
		ZoneController
	])
	.controller('FormatsController', [
		'$scope',
		'$routeParams',
		'FormatResource',
		'TemplateResource',
		'ResourceResource',
		FormatsController
	])
	.controller('NewFormatController', [
		'$routeParams',
		'FormatResource',
		'TemplateResource',
		NewFormatController
	])
	.controller('SpoolersController', [
		'$scope',
		'$routeParams',
		'SpoolerResource',
		SpoolersController
	])
	.controller('NewSpoolerController', [
		'$routeParams',
		'SpoolerResource',
		NewSpoolerController
	])
	.controller('TerminalsController', [
		'$scope',
		'$routeParams',
		'TerminalResource',
		TerminalsController
	])
	.controller('NewTerminalController', [
		'$routeParams',
		'TerminalResource',
		NewTerminalController
	])
	.controller('RoutesController', [
		'$scope',
		'$routeParams',
		'TerminalResource',
		'ZoneResource',
		'SpoolerResource',
		RoutesController
	])
	.controller('NewTerminalRouteController', [
		'$routeParams',
		'SpoolerResource',
		'TerminalResource',
		NewTerminalRouteController
	])
	.controller('NewZoneRouteController', [
		'$routeParams',
		'ZoneResource',
		'SpoolerResource',
		NewZoneRouteController
	])
	.name;