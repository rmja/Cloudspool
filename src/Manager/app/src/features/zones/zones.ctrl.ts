import { Zone } from '../../api/zones';
import {ZoneResource} from '../../resources';

export class ZonesController {
	zones: Array<Zone>;

	constructor($scope: ng.IScope, private ZoneResource: ZoneResource) {
		this.zones = ZoneResource.cache.getAll();

		$scope.$watch(ZoneResource.cache.getLastModified,() => {
			this.zones = ZoneResource.cache.getAll();
		});
	}

	saveZone(zone: Zone) {
		this.ZoneResource.update(zone.id, [{op: "replace", path: "/name", value: zone.name}]);
	}

	destroy(zone: Zone) {
		this.ZoneResource.destroy(zone.id);
	}
}

export class NewZoneController {
	name: string;

	constructor(private ZoneResource: ZoneResource) {
	}

	submit() {
		this.ZoneResource.create({
			name: this.name
		});
	}
}