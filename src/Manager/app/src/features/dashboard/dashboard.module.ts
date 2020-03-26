import {DashboardController} from './dashboard.ctrl';
import {routing} from './dashboard.config';

export default angular.module('app.dashboard', [])
    .config(routing)
    .controller('DashboardController', [
        DashboardController
    ])
    .name;