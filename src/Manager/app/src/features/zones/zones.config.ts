import {FormatResource, ResourceResource, SpoolerResource, TemplateResource, TerminalResource, ZoneResource} from '../../resources';

export function routing($routeProvider) {
    $routeProvider
        .when('/zones', {
            template: require('./zones.tmpl'),
            controller: 'ZonesController',
            controllerAs: 'ctrl',
            resolve: {
                zones: (ZoneResource: ZoneResource) => {
                    return ZoneResource.getAll();
                }
            }
        })
        .when('/zones/:zoneId', {
            template: require('./zone/zone.tmpl'),
            controller: 'ZoneController',
            controllerAs: 'ctrl',
            resolve: {
                zone: ($route, ZoneResource: ZoneResource) => {
                    var zoneId = parseInt($route.current.params.zoneId);
                    return ZoneResource.getById(zoneId);
                },
                formats: ($route, FormatResource: FormatResource) => {
                    var zoneId = parseInt($route.current.params.zoneId);
                    return FormatResource.getAll(zoneId);
                },
                templates: (TemplateResource: TemplateResource) => {
                    return TemplateResource.getAll();
                },
                resources: (ResourceResource: ResourceResource) => {
                    return ResourceResource.getAll();
                },
                terminals: ($route, TerminalResource: TerminalResource) => {
                    var zoneId = parseInt($route.current.params.zoneId);
                    return TerminalResource.getAllByZoneId(zoneId);
                },
                spoolers: ($route, SpoolerResource: SpoolerResource) => {
                    var zoneId = parseInt($route.current.params.zoneId);
                    return SpoolerResource.getAllByZoneId(zoneId);
                }
            }
        });
}