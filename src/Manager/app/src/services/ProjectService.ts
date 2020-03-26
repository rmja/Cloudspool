export class ProjectService {
	private _projectKey: string;

	private storageKey = 'cloudspool_project_key';

	get projectKey() {
		return this._projectKey;
	}

	constructor(private $window: ng.IWindowService) {
		this._projectKey = $window.sessionStorage.getItem(this.storageKey);
	}

	public isLoggedIn() {
		return this._projectKey != null;
	}

	public logIn(projectKey: string) {
		this._projectKey = projectKey;
		this.$window.sessionStorage.setItem(this.storageKey, projectKey);
	}

	public logOut() {
		this._projectKey = null;
		this.$window.sessionStorage.removeItem(this.storageKey);
	}
}