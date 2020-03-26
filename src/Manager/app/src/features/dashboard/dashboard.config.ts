export function routing($routeProvider) {
    $routeProvider
        .when('/dashboard', {
            template: require('./dashboard.tmpl'),
            controller: 'DashboardController',
            controllerAs: 'ctrl'
        });
}