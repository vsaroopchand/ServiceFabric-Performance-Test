

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
      useValue: 'http://52.184.162.93:8080/52411a5a-2cf8-431c-9a44-69f7b854b423/131480682769426078/845202fa-097d-4d98-9e01-9bac411c79e9'
      //useValue: 'http://localhost:8080/dbcd4397-f5f9-41cc-884c-e9aa449afa5b/131480150046107502/95353b4a-5b58-4709-951d-6daa0bf87f4b'
    },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
