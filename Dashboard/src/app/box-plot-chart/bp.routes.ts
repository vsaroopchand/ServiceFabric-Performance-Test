import { BoxPlotChartContainer } from './containers/bp.container';
import { RouterModule } from '@angular/router';
export const routes = RouterModule.forChild([
    { path: 'bp', component: BoxPlotChartContainer }
]);