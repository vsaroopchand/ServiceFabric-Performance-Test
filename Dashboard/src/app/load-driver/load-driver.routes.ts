import { LoadDriverContainer } from './containers/load-driver';
import { RouterModule } from '@angular/router';


export const routes = RouterModule.forChild([{
    path: 'load',
    component: LoadDriverContainer
}]);