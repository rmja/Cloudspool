import {OrFilter} from './filters/orFilter';

export default angular.module('filters', [])
    .filter('orFilter', [
        () => {
            return new OrFilter().filter;
        }
    ])
    .name;