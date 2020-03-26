export class OrFilter {
	public filter(items, props) {
		var out = [];

		if (angular.isArray(items)) {
			items.forEach(item => {
				if (deepCompare(item, props)) {
					out.push(item);
				}
			});
		}
		else {
			out = items;
		}

		return out;

		function deepCompare(item, props) {
			var keys = Object.keys(props);
			for (var i = 0; i < keys.length; i++) {
				var prop = keys[i];
				var value = props[prop];
				if (angular.isObject(value)) {
					if (deepCompare(item[prop], value)) {
						return true;
					}
				}
				else {
					var text = value.toLowerCase();
					if (item[prop].toString().toLowerCase().indexOf(text) !== -1) {
						return true;
					}
				}
			}
			return false;
		}
	}
}