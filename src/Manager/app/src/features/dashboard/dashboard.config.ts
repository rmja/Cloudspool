import {ZoneResource} from '../../resources';

export function routing($routeProvider) {
    $routeProvider
        .when('/dashboard', {
            template: require('./dashboard.tmpl'),
            controller: 'DashboardController',
            controllerAs: 'ctrl',
            resolve: {
                zones: (ZoneResource: ZoneResource) => {
                    return ZoneResource.getAll();
                }
            }
        });
}