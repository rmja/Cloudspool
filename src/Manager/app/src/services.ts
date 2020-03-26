import {AuthInterceptorService} from './services/AuthInterceptorService';
import {FileReaderService} from './services/FileReaderService';
import {ProjectService} from './services/ProjectService';

export {
    AuthInterceptorService,
    FileReaderService,
    ProjectService
};

export default angular.module('services', [])
    .service('AuthInterceptorService', [
        'ProjectService',
        AuthInterceptorService
    ])
    .service('FileReaderService', [
        '$q',
        FileReaderService
    ])
    .service('ProjectService', [
        '$window',
        ProjectService
    ])
    .name;