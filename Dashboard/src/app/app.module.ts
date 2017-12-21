

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
      //useValue: 'http://13.82.61.190:8080/64fd5d32-31f6-49b5-8a1b-fd33a40cb3dc/131542365840991406/08c8bdaa-1f71-4c2c-a759-8343c4526b19'
      useValue: 'http://localhost:8080/9f36f768-1ddb-4b62-84ed-1020a5b26fc1/131583445270994766/1cdebc41-9c1d-467f-b317-db6fd372d5e2'
    },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
