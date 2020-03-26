import {TemplateResource} from '../../resources';

export function routing($routeProvider) {
    $routeProvider
        .when('/templates', {
            template: require('./templates.tmpl'),
            controller: 'TemplatesController',
            controllerAs: 'ctrl',
            resolve: {
                templates: (TemplateResource: TemplateResource) => {
                    return TemplateResource.getAll();
                }
            }
        }).when('/templates/:templateId', {
            template: require('./template/template.tmpl'),
            controller: 'TemplateController',
            controllerAs: 'ctrl',
            resolve: {
                template: ($route, TemplateResource: TemplateResource) => {
                    var templateId = parseInt($route.current.params.templateId);
                    return TemplateResource.getById(templateId);
                },
                script: ($route, TemplateResource: TemplateResource) => {
                    var templateId = parseInt($route.current.params.templateId);
                    return TemplateResource.getScriptById(templateId);
                }
            }
        });
}