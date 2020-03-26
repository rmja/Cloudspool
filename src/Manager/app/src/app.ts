import { ProjectService } from './services/ProjectService';
import { Http } from 'ur-http';
import directives from './directives';
import filters from './filters';
import index from './features/index/index.module';
import dashboardFeature from './features/dashboard/dashboard.module';
import zonesFeature from './features/zones/zones.module';
import templatesFeature from './features/templates/templates.module';
import resourcesFeature from './features/resources/resources.module';
import resources from './resources';
import services from './services';

// Underscore String Module.
_.str = (<any>window).s;

const apiBaseBath = window.location.hostname === "localhost" ? 'https://localhost:51331' : '/api';
let resolvedProjectService: ProjectService;
Http.defaults.fetch = (input, init) => {
    const projectKey = resolvedProjectService && resolvedProjectService.projectKey;
    if (projectKey) {
        const headers: any = init.headers;
        headers.set("Authorization", `Bearer project:${projectKey}`);
    }

    if (typeof input === "string") {
        return (<any>window).fetch(apiBaseBath + input, init);
    }
    return (<any>window).fetch(input, init);
}

angular.module('ManagerApplication', ['js-data', 'ngSanitize', 'xeditable', 'ui.bootstrap', 'ui.ace', 'ui.select', 'angularUtils.directives.dirPagination', 'ngRoute', 'file-model', index, directives, filters, dashboardFeature, zonesFeature, templatesFeature, resourcesFeature, resources, services])
    .constant('apiBasePath', apiBaseBath)
    .constant('contentTypes', ['application/pdf', 'application/x-escp', 'application/x-starlinemode'])
    .constant('jsonAceOptions', {
        useSoftTabs: true,
        tabSize: 4,
        showInvisibles: false,
        mode: 'json'
    })
    .run((editableOptions, editableThemes) => {
        editableThemes.bs3.submitTpl = '<button type="submit" class="btn btn-primary"><span class="fa fa-save"></span></button>';
        editableThemes.bs3.cancelTpl = '<button type="button" class="btn btn-default" ng-click="$form.$cancel()">' +
            '<span class="fa fa-undo"></span>' +
            '</button>'
        editableOptions.theme = 'bs3'; // bootstrap3 theme. Can be also 'bs2', 'default'
    })
    .config((DSProvider, apiBasePath) => {
        DSProvider.defaults.basePath = apiBasePath;
    })
    .config(($httpProvider: ng.IHttpProvider) => {
        $httpProvider.interceptors.push('AuthInterceptorService');
    })
    .config(uiSelectConfig => {
        uiSelectConfig.theme = 'bootstrap';
    })
    .config(paginationTemplateProvider => {
        paginationTemplateProvider.setPath('lib/angular-utils-pagination/dirpagination.tpl.html');
    })
    .config(($routeProvider: ng.route.IRouteProvider) => {
        $routeProvider.otherwise('/dashboard');
    })
    .run(["ProjectService", (projectService: ProjectService) => {
        resolvedProjectService = projectService;
    }]);