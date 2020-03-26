import { Terminal } from '../../../api/terminals';
import {TerminalResource} from '../../../resources';

export class TerminalsController {
	terminals: Array<Terminal>;

	constructor($scope: ng.IScope, $routeParams, private TerminalResource: TerminalResource) {
		var zoneId = parseInt($routeParams.zoneId);

		$scope.$watch(TerminalResource.cache.getLastModified,() => {
			this.terminals = TerminalResource.cache.getAll(x => x.zoneId === zoneId);
		});
	}

	saveTerminal(terminal: Terminal) {
		this.TerminalResource.update(terminal.id, [
			{ op: "replace", path: "/name", value: terminal.name }
		]);
	}

	destroy(terminal: Terminal) {
		this.TerminalResource.destroy(terminal.id);
	}
}

export class NewTerminalController {
	zoneId: number;
	name: string;

	constructor($routeParams, private TerminalResource: TerminalResource) {
		this.zoneId = parseInt($routeParams.zoneId);
	}

	submit() {
		this.TerminalResource.create({
			zoneId: this.zoneId,
			name: this.name
		});
	}
}