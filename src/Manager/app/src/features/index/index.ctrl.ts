import {ProjectResource} from '../../resources';
import {ProjectService} from '../../services';

export class IndexController {
    projectKey: string;
    showError = false;

    projectName: string;

    get isLoggedIn() {
        return this.ProjectService.isLoggedIn();
    }

    constructor(private $route, private ProjectResource: ProjectResource, private ProjectService: ProjectService) {
        if (ProjectService.isLoggedIn()) {
            ProjectResource.getByKey(ProjectService.projectKey).then(project => {
                this.projectName = project.name;
            });
        }
    }

    logIn() {
        this.showError = false;
        this.ProjectResource.getByKey(this.projectKey).then((project) => {
            this.showError = false;
            this.projectName = project.name;
            this.ProjectService.logIn(project.key);
            this.$route.reload();
        }).catch(() => {
            this.showError = true;
            this.projectName = null;
        });
    }

    logOut() {
        this.ProjectService.logOut();
    }
}