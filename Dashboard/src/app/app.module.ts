

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
      useValue: 'http://52.184.162.93:8080/4b830bfd-bfc6-47d1-b551-e2d8e183964d/131479019334563073/30ac3fb8-c85d-4325-bb9e-0e5dfa9d4af8'
      //useValue: 'http://localhost:8080/717c71fb-00fb-4f4b-bc52-375b7b095dc1/131479025157455780/dc75ed6d-ef5f-4e99-8c61-0da0a7f99410'
    },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
