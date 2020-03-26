import { SpoolerResource, TerminalResource } from '../../../resources';
import { Terminal, TerminalRoute } from '../../../api/terminals';

import { ZoneResource } from './../../../resources/ZoneResource';
import { ZoneRoute } from '../../../api/zones';

export class RoutesController {
	zoneId: number;
	zoneRoutes: Array<RouteViewModel>;
	allTerminalRoutes: Array<RouteViewModel>;
	standaloneTerminalRoutes: Array<RouteViewModel>;

	get showWelcome() {
		var terminals = this.TerminalResource.cache.getAll(x => x.zoneId === this.zoneId);
		var spoolers = this.SpoolerResource.cache.getAll(x => x.zoneId === this.zoneId);
		var printers = [];
		_.each(spoolers, spooler => {
			_.each(spooler.printers, printer => {
				printers.push(printer);
			});
		});

		return terminals.length == 0 || spoolers.length == 0 || printers.length == 0;
	}

	constructor($scope: ng.IScope, $routeParams,
		private TerminalResource: TerminalResource,
		private ZoneResource: ZoneResource,
		private SpoolerResource: SpoolerResource) {
		this.zoneId = parseInt($routeParams.zoneId);

		$scope.$watch(ZoneResource.cache.getLastModified, () => {
			this.zoneRoutes = Object.values(ZoneResource.cache.get(this.zoneId).routes).map(route => {
				var vm: RouteViewModel = {
					kind: "zone",
					zoneId: route.zoneId,
					alias: route.alias,
					spoolerId: route.spoolerId,
					printerName: route.printerName
				};
				return vm;
			});
		});

		this.allTerminalRoutes = [];
		this.standaloneTerminalRoutes = [];
		$scope.$watch(() => {
			return ZoneResource.cache.getLastModified().valueOf() + TerminalResource.cache.getLastModified().valueOf();
		}, () => {
			this.allTerminalRoutes.length = 0;
			this.standaloneTerminalRoutes.length = 0;
			_.each(TerminalResource.cache.getAll(x => x.zoneId === this.zoneId), terminal => {
				_.each(terminal.routes, terminalRoute => {
					var defaultRouteExists = _.any(this.zoneRoutes, zoneRoute => {
						return zoneRoute.alias == terminalRoute.alias;
					});
					if (!defaultRouteExists) {
						this.standaloneTerminalRoutes.push({
							kind: "terminal",
							terminalId: terminalRoute.terminalId,
							alias: terminalRoute.alias,
							spoolerId: terminalRoute.spoolerId,
							printerName: terminalRoute.printerName
						});
					}
					this.allTerminalRoutes.push({
						kind: "terminal",
						terminalId: terminalRoute.terminalId,
						alias: terminalRoute.alias,
						spoolerId: terminalRoute.spoolerId,
						printerName: terminalRoute.printerName
					});
				});
			});
		});
	}

	saveRoute(route: RouteViewModel) {
		switch (route.kind) {
			case "terminal":
				this.TerminalResource.update(route.terminalId, [
					{ op: "replace", path: `/routes/${route.alias}`, value: route }
				]);
				break;
			case "zone":
				this.ZoneResource.update(route.zoneId, [
					{ op: "replace", path: `/routes/${route.alias}`, value: route }
				]);
				break;
		}
	}

	getPrinterName(route: RouteViewModel) {
		return route.printerName;
	}

	getSpoolerName(route: RouteViewModel) {
		var spooler = this.SpoolerResource.cache.get(route.spoolerId);
		return spooler.name;
	}

	getTerminalName(terminalRoute: TerminalRoute) {
		var terminal = this.TerminalResource.cache.get(terminalRoute.terminalId);
		return terminal.name;
	}

	destroyZoneRoute(zoneRoute: ZoneRoute) {
		this.ZoneResource.update(zoneRoute.zoneId, [
			{ op: "remove", path: `/routes/${zoneRoute.alias}` }
		]);
	}

	destroyTerminalRoute(terminalRoute: TerminalRoute) {
		this.TerminalResource.update(terminalRoute.terminalId, [
			{ op: "remove", path: `/routes/${terminalRoute.alias}` }
		]);
	}
}

export class NewZoneRouteController {
	zoneId: number;
	printers: Array<PrinterViewModel>;
	selectedPrinter: PrinterViewModel;
	alias: string;

	errorMessage: string;

	get showError() {
		return this.errorMessage != null;
	}

	constructor($routeParams,
		private ZoneResource: ZoneResource,
		private SpoolerResource: SpoolerResource) {
		this.zoneId = parseInt($routeParams.zoneId);

		this.printers = [];
		_.each(SpoolerResource.cache.getAll(x => x.zoneId === this.zoneId), (spooler) => {
			_.each(spooler.printers, printerName => {
				this.printers.push({ spoolerId: spooler.id, printerName: printerName });
			});
		});
		this.selectedPrinter = _.first(this.printers);

		this.getSpoolerName = this.getSpoolerName.bind(this);
	}

	getSpoolerName(printer: PrinterViewModel) {
		var spooler = this.SpoolerResource.cache.get(printer.spoolerId);
		return spooler.name;
	}

	submit() {
		this.errorMessage = null;
		const route: ZoneRoute = {
			zoneId: this.zoneId,
			alias: this.alias,
			spoolerId: this.selectedPrinter.spoolerId,
			printerName: this.selectedPrinter.printerName
		};
		this.ZoneResource.update(this.zoneId, [
			{ op: "add", path: `/routes/${route.alias}`, value: route }
		]).catch((error) => {
			this.errorMessage = error.data;
		});
	}
}

export class NewTerminalRouteController {
	zoneId: number;
	terminals: Array<Terminal>;
	selectedTerminal: Terminal;
	printers: Array<PrinterViewModel>;
	selectedPrinter: PrinterViewModel;
	alias: string;

	errorMessage: string;

	get showError() {
		return this.errorMessage != null;
	}

	constructor($routeParams,
		private SpoolerResource: SpoolerResource,
		private TerminalResource: TerminalResource) {
		this.zoneId = parseInt($routeParams.zoneId);

		this.printers = [];
		_.each(SpoolerResource.cache.getAll(x => x.zoneId === this.zoneId), (spooler) => {
			_.each(spooler.printers, printerName => {
				this.printers.push({ spoolerId: spooler.id, printerName: printerName });
			});
		});
		this.selectedPrinter = _.first(this.printers);

		this.terminals = TerminalResource.cache.getAll(x => x.zoneId === this.zoneId);
		this.selectedTerminal = _.first(this.terminals);

		this.getSpoolerName = this.getSpoolerName.bind(this);
	}

	getSpoolerName(printer: PrinterViewModel) {
		var spooler = this.SpoolerResource.cache.get(printer.spoolerId);
		return spooler.name;
	}

	submit() {
		this.errorMessage = null;
		const route: TerminalRoute = {
			terminalId: this.selectedTerminal.id,
			alias: this.alias,
			spoolerId: this.selectedPrinter.spoolerId,
			printerName: this.selectedPrinter.printerName
		};
		this.TerminalResource.update(this.selectedTerminal.id, [
			{ op: "add", path: `/routes/${route.alias}`, value: route }
		]).catch((error) => {
			this.errorMessage = error.data;
		});
	}
}

type RouteViewModel = {
	kind: "zone";
	zoneId: number;
	alias: string;
	spoolerId: number;
	printerName: string;
} | {
	kind: "terminal";
	terminalId: number;
	alias: string;
	spoolerId: number;
	printerName: string;
}

interface PrinterViewModel {
	spoolerId: number;
	printerName: string;
}