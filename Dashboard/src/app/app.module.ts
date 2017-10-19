

import { AppNavComponent } from './app.nav.component';
import { NvD3Module } from 'ng2-nvd3';

import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SharedServicesModule } from './shared/shared.module';
import { BaseEndpoint } from './app.constants';
import { BoxPlotModule } from './box-plot-chart/bp.module';
import { LoadDriverModule } from './load-driver/load-driver.module';


@NgModule({
  declarations: [
    AppComponent,
    AppNavComponent
  ],
  imports: [
    BrowserModule,
    HttpModule,
    NvD3Module,
    AppRoutingModule,
    SharedServicesModule,
    BoxPlotModule,
    LoadDriverModule
  ],
  exports: [LoadDriverModule],
  providers: [
    {
      provide: BaseEndpoint,
      //useValue: 'http://40.76.62.210:8080/39d09740-530b-4b4c-bde0-384246a95635/131509102027776997/a0a3baab-44b0-4fc8-a984-77022834670d'
      useValue: 'http://localhost:8080/44b7e882-1198-4cb7-b412-51edae614513/131529102783845834/b5ee8d6e-f578-47a6-a991-a60dc900f917'
    },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
