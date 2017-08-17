import { Component, Input } from '@angular/core';

@Component({
    selector: 'my-chart',
    template: `
        <nvd3 [options]="options" [data]="data"></nvd3>
    `
})
export class Nvd3BoxPlotWrapper {

    @Input() options: any;
    @Input() data: any;

    constructor() {
        // wrapper over the nvd3 chart so we can dynamically load this chart
        // the nvd3 component is not bootstrappable, hence it cannot be loaded dynamically
    }
}