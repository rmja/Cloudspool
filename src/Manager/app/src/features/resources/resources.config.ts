import {ResourceResource} from '../../resources';

export function routing($routeProvider) {
    $routeProvider
        .when('/resources', {
            template: require('./resources.tmpl'),
            controller: 'ResourcesController',
            controllerAs: 'ctrl',
            resolve: {
                resources: ($q: ng.IQService, ResourceResource: ResourceResource) => {
                    return ResourceResource.getAll();
                }
            }
        }).when('/resources/:resourceAlias', {
            template: require('./resource/resource.tmpl'),
            controller: 'ResourceController',
            controllerAs: 'ctrl',
            resolve: {
                resource: ($route, ResourceResource: ResourceResource) => {
                    var resourceAlias = $route.current.params.resourceAlias;
                    return ResourceResource.getByAlias(resourceAlias);
                },
                resourceContent: ($route, ResourceResource: ResourceResource) => {
                    var resourceAlias = $route.current.params.resourceAlias;
                    return ResourceResource.getJsonContent(resourceAlias);
                }
            }
        });
}