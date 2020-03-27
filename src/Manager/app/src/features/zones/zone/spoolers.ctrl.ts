import { Spooler } from '../../../api/spoolers';
import { SpoolerResource } from '../../../resources';

export class SpoolersController {
	spoolers: Array<Spooler>;

	constructor($scope: ng.IScope, $routeParams, private SpoolerResource: SpoolerResource) {
		var zoneId = parseInt($routeParams.zoneId);
		$scope.$watch(SpoolerResource.cache.getLastModified, () => {
			this.spoolers = SpoolerResource.cache.getAll(x => x.zoneId === zoneId);
		});
	}

	refreshPrinters(spooler: Spooler) {
		this.SpoolerResource.getById(spooler.id, true);
	}

	saveSpooler(spooler: Spooler) {
		this.SpoolerResource.update(spooler.id, [
			{ op: "replace", path: "/name", value: spooler.name }
		]);
	}

	destroy(spooler: Spooler) {
		this.SpoolerResource.destroy(spooler.id);
	}

	anyPrinters(spooler: Spooler) {
		return spooler.printers.length > 0;
	}

	getPrinterNames(spooler: Spooler) {
		return spooler.printers;
	}
}

export class NewSpoolerController {
	zoneId: number;
	name: string;

	constructor($routeParams, private SpoolerResource: SpoolerResource) {
		this.zoneId = parseInt($routeParams.zoneId);
	}

	submit() {
		this.SpoolerResource.create({
			zoneId: this.zoneId,
			name: this.name
		});
	}
}