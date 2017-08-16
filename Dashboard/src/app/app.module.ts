import { AppNavComponent } from './app.nav.component';
import { NvD3Module } from 'ng2-nvd3';

import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { SharedServicesModule } from './shared/shared.module';
import { BaseEndpoint } from './app.constants';
import { BoxPlotModule } from './box-plot-chart/bp.module';



@NgModule({
  declarations: [
    AppComponent,
    AppNavComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule,
    NvD3Module,
    AppRoutingModule,
    SharedServicesModule,
    BoxPlotModule
  ],
  providers: [
    { provide: BaseEndpoint, useValue: 'http://localhost:8080/f09198ec-a076-49ec-bd82-e895486ca4c2/131473746611001256/ca8fed07-f250-4c2a-9b5d-ef6a9b9577f0' },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
