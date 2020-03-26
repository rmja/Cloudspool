import {ProjectService} from '../services';

export class AuthInterceptorService {
	constructor(private ProjectService: ProjectService) {
		this.request = this.request.bind(this);
	}

	public request(config: ng.IRequestConfig) {
		if (this.ProjectService.projectKey != null) {
			config.headers['Authorization'] = "Bearer project:" + this.ProjectService.projectKey;
		}
		return config;
	}
}