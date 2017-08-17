import { routes } from './load-driver.routes';
import { LoadDriverComponent } from './components/load-driver';

import { LoadDriverContainer } from './containers/load-driver';

import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@NgModule({
    declarations: [LoadDriverContainer, LoadDriverComponent],
    imports: [CommonModule, FormsModule, ReactiveFormsModule, routes],
    exports: [LoadDriverContainer, LoadDriverComponent]
})
export class LoadDriverModule {

}