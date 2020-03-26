import {IndexController} from './index.ctrl';

export default angular.module('app.index', [])
    .controller('IndexController', [
        '$route',
        'ProjectResource',
        'ProjectService',
        IndexController
    ])
    .name;