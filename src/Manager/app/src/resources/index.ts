import { FormatResource } from './FormatResource';
import { ProjectResource } from './ProjectResource';
import { ResourceResource } from './ResourceResource';
import { SpoolerResource } from './SpoolerResource';
import { TemplateResource } from './TemplateResource';
import { TerminalResource } from './TerminalResource';
import { ZoneResource } from './ZoneResource';

export {
    FormatResource,
    ProjectResource,
    ResourceResource,
    SpoolerResource,
    TemplateResource,
    TerminalResource,
    ZoneResource
}

export default angular.module('resources', [])
    .service('FormatResource', [
        '$rootScope',
        FormatResource
    ]).run(['FormatResource', (service) => { }])
    .service('ProjectResource', [
        '$rootScope',
        ProjectResource
    ]).run(['ProjectResource', (service) => { }])
    .service('ResourceResource', [
        '$rootScope',
        'FileReaderService',
        ResourceResource
    ]).run(['ResourceResource', (service) => { }])
    .service('SpoolerResource', [
        '$rootScope',
        SpoolerResource
    ]).run(['SpoolerResource', (service) => { }])
    .service('TemplateResource', [
        '$rootScope',
        TemplateResource
    ]).run(['TemplateResource', (service) => { }])
    .service('TerminalResource', [
        '$rootScope',
        TerminalResource
    ]).run(['TerminalResource', (service) => { }])
    .service('ZoneResource', [
        '$rootScope',
        ZoneResource
    ]).run(['ZoneResource', (service) => { }])
    .name;
