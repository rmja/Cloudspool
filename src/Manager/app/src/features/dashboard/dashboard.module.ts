import {routing} from './dashboard.config';
import {DashboardController} from './dashboard.ctrl';

export default angular.module('app.dashboard', [])
    .config(routing)
    .controller('DashboardController', [
        'zones',
        DashboardController
    ])
    .name;