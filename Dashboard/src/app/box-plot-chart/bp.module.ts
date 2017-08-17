import { Nvd3BoxPlotWrapper } from './components/nvd3-wrapper.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { routes } from './bp.routes';
import { BrowserModule } from '@angular/platform-browser';
import { BoxPlotChartContainer } from './containers/bp.container';
import { BoxPlotChart } from './components/bp.component';
import { NgModule } from '@angular/core';
import { NvD3Module } from 'ng2-nvd3';
import { LoadDriverModule } from './../load-driver/load-driver.module';

@NgModule({
    imports: [CommonModule, BrowserModule, FormsModule, NvD3Module, LoadDriverModule, routes],
    declarations: [BoxPlotChartContainer, BoxPlotChart, Nvd3BoxPlotWrapper],
    exports: [BoxPlotChartContainer],
    bootstrap: [BoxPlotChartContainer, Nvd3BoxPlotWrapper],
    providers: []
})
export class BoxPlotModule {

}