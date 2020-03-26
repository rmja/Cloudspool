export class FileReaderService {

	constructor(private $q: ng.IQService) {
	}

	public readBinary(file: File): ng.IPromise<Uint8Array> {
		var defer = this.$q.defer();

		if (FileReader) {
			var reader = new FileReader();

			if (reader && reader.onload !== undefined && reader.readAsArrayBuffer) {
				reader.onload = (e) => {
					defer.resolve(new Uint8Array(reader.result));
				};
				reader.onerror = (e) => {
                    defer.reject(e);
					//defer.reject(e.message);
				};
				reader.readAsArrayBuffer(file);
			}
			else {
				defer.reject('FileReader is not have the requested functionality. Please update the browser.');
			}
		}
		else {
			defer.reject('FileReader is not supported in the browser. Please update the browser.');
		}

		return defer.promise;
	}

	public readText(file: File): ng.IPromise<string> {
		var defer = this.$q.defer();

		if (FileReader) {
			var reader = new FileReader();

			if (reader && reader.onload !== undefined && reader.readAsText) {
				reader.onload = (e) => {
					defer.resolve(reader.result);
				};
				reader.onerror = (e) => {
                    defer.reject(e);
					//defer.reject(e.message);
				};
				reader.readAsText(file);
			}
			else {
				defer.reject('FileReader is not have the requested functionality. Please update the browser.');
			}
		}
		else {
			defer.reject('FileReader is not supported in the browser. Please update the browser.');
		}

		return defer.promise;
	}
}