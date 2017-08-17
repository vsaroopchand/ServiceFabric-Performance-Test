import { Component, OnChanges, Output, Input, EventEmitter, SimpleChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
    selector: 'my-load-driver',
    template: `
    
    <form [formGroup]="form">
        <div class="row">
            <div class="col-md-6">
                <div class="form-group">
                    <label for="requests"># of Requests (for load)</label>
                    <input formControlName="requests" id="requests" type="number" class="form-control"
                        required="required">                
                </div>
            </div>
     
        </div> 
        <div class="row">
            <div class="col-md-6">                
                <button type="button" class="btn btn-default" (click)="resetSession()">Reset</button>
                <button type="button" class="btn btn-primary" (click)="startLoad()" [disabled]="!allowStart">Start</button>
            </div>
        </div>   
    </form>
    `
})
export class LoadDriverComponent implements OnChanges {

    @Input() form: FormGroup;
    @Output('onStartCallback') onStartCallback: EventEmitter<any> = new EventEmitter<any>();
    @Output('onResetCallback') onResetCallback: EventEmitter<any> = new EventEmitter<any>();

    constructor() {

    }

    ngOnChanges(changes: SimpleChanges) {

    }

    startLoad() {
        if (this.onStartCallback && this.form.valid) {
            this.onStartCallback.emit({});
        }
    }

    resetSession() {
        this.onResetCallback && this.onResetCallback.emit({});
    }

    get allowStart() {
        return this.form.valid;
    }
}