import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { routes } from './bp.routes';
import { BrowserModule } from '@angular/platform-browser';
import { BoxPlotChartContainer } from './containers/bp.container';
import { BoxPlotChart } from './components/bp.component';
import { NgModule } from '@angular/core';
import { NvD3Module } from 'ng2-nvd3';

@NgModule({
    imports: [CommonModule , BrowserModule, FormsModule, NvD3Module, routes],
    declarations: [BoxPlotChartContainer, BoxPlotChart],
    exports: [BoxPlotChartContainer],
    bootstrap: [BoxPlotChartContainer],
    providers: []
})
export class BoxPlotModule {

}