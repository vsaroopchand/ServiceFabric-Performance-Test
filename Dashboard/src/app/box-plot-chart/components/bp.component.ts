import { ResultService } from './../../shared/services/result.service';
import { Component, OnInit, OnDestroy } from '@angular/core';

declare let d3, nv: any;

@Component({
    selector: 'bp-chart',
    template: `
    <form class="form-inline">
    <div class="row" >
        <div class="col-md-6">
            <div class="form-group">
                <label for="view-options">View</label>
                <select id="view-options" 
                    class="form-control" 
                    [(ngModel)]="selectedView" 
                    (ngModelChange)="updateSelectedView($event)" 
                    [ngModelOptions]="{standalone: true}">
                    <option *ngFor="let o of viewOptions" [ngValue]="o.id">{{o.label}}</option>
                </select>                
            </div>
        </div>
        <div class="col-md-6">
            <div class="form-group">
      
            </div>
        </div>
    </div>
    </form>
    <div class="row">
    <nvd3 [options]="options" [data]="data"></nvd3>
    </div>
    `,
})
export class BoxPlotChart implements OnInit, OnDestroy {


    selectedView: any = 1;
    viewOptions: any = [];

    options: any = {
        chart: {
            type: 'boxPlotChart',
            height: 450,
            margin: {
                top: 20,
                right: 20,
                bottom: 50,
                left: 50
            },
            xAxis: {
                axisLabel: 'Commication Stack',
            },
            yAxis: {
                axisLabel: 'Call Duration (seconds)',
                axisLabelDistance: -10
            },
            tooltip: {
                valueFormatter: function (d) {
                    // return 'what is this';
                    return d;
                },
                keyFormatter: function (d) {
                    //return 'Key';
                    return d;
                }
            },
            color: ['#31a354', '#de2d26', '#2b8cbe', '#756bb1', '#99d8c9'],
            x: function (d) { return d.label; },
            y: function (d) { return d.values.S1; },
            maxBoxWidth: 200,
            yDomain: [0, 3]
        }
    }

    data: any = [];


    handle: any;
    constructor(private resultService: ResultService) {
        this.viewOptions = [
            { id: 0, label: 'Avg. All' },
            { id: 1, label: 'Avg. Top 10' }
        ];

    }

    ngOnInit() {
        /*
                this.handle = setInterval(
                    this.resultService.getBoxPlotData().subscribe(r => {
                        console.log(r);
                        this.data = r;
        
                    }), 5000);
        */
        this.loadChartData();
    }

    loadChartData() {
        switch (this.selectedView) {
            case 0:
                this.data = [];
                break;
            case 1:
                this.resultService.getBoxPlotData().subscribe(r => {
                    console.log(r);
                    this.data = r;

                });
                break;
        }
    }

    ngOnDestroy() {
        //clearInterval(this.handle);
    }

    updateSelectedView(event: string): void {
        this.loadChartData();
    }
}