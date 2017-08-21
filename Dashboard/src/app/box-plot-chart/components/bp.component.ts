import { Nvd3BoxPlotWrapper } from './nvd3-wrapper.component';
import { SessionService } from './../../shared/services/session.service';
import { ResultService } from './../../shared/services/result.service';
import { Component, OnInit, OnDestroy, ViewContainerRef, ViewChild, ComponentFactoryResolver } from '@angular/core';

declare let d3, nv: any;

@Component({
    selector: 'bp-chart',
    template: `
    <div class="row">
        <my-load-driver-container (onResetCallback)="reset(e)"></my-load-driver-container>
    </div>
    <br />
    <div class="row">
        <form class="form-horizontal">           
                <div class="col-md-2">
                    <div class="form-group">
                        <label for="view-options">View</label>
                        <select id="view-options" 
                            class="form-control " 
                            [(ngModel)]="selectedView" 
                            (ngModelChange)="updateSelectedView($event)" 
                            [ngModelOptions]="{standalone: true}">
                            <option *ngFor="let o of viewOptions" [ngValue]="o.id">{{o.label}}</option>
                        </select>                
                    </div>                
                </div>
                <div class="col-md-offset-1 col-md-5">
                    <div class="form-group">
                        <label for="session-options">Session</label>
                        <select id="session-options" 
                            class="form-control" 
                            [(ngModel)]="selectedSession" 
                            (ngModelChange)="updateSelectedView($event)" 
                            [ngModelOptions]="{standalone: true}">
                            <option *ngFor="let o of this.sessionService.sessions" [ngValue]="o">{{o}}</option>
                        </select>       
                    </div>
                </div>
                <div class="col-md-offset-1 col-md-1">
                    <div class="form-group">
                    <label for="session-options">Auto Refresh</label>
                        <div class="checkbox form-control">
                            <label>                                
                                <input type="checkbox" 
                                (change)="autoRefreshChanged()"
                                [(ngModel)]="autoRefresh" 
                                [ngModelOptions]="{standalone: true}"> {{autoRefresh ? 'On' : 'Off' }}
                            </label>
                        </div>
                    </div>    
                </div>                
                <div class="col-md-offset-1 col-md-1">
                    <div class="form-group">
                        <label for="yscale-options">Y Scale (Max)</label>                             
                        <select id="yscale-options" 
                            class="form-control " 
                            [(ngModel)]="yScaleMax"  
                            (change)="setYScaleMax()"       
                            [ngModelOptions]="{standalone: true}">
                            <option *ngFor="let o of yScaleRange" [ngValue]="o">{{o}}</option>
                        </select>                                            
                    </div>    
            </div>      

        </form>
    </div>
    <div class="row" #chart>
        
    </div>
    `,
})
export class BoxPlotChart implements OnInit, OnDestroy {

    // auto refresh checkbox and interval
    autoRefresh = false;
    timerHandle: any;

    // yScale dropdown selector
    yScaleMax = 1;
    yScaleRange: Array<number> = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

    // chart options
    selectedView: any = 1;
    viewOptions: any = [];

    // sesson selection
    selectedSession: any;

    // chart options
    options: any = {
        chart: {
            type: 'boxPlotChart',
            height: 350,
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
                axisLabel: 'Call Duration (ms)',
                axisLabelDistance: -10
            },
            tooltip: {
                valueFormatter: function (d) {
                    if (isNaN(d)) {
                        return d;
                    }
                    else {
                        return parseFloat(d).toFixed(0);
                    }
                },
                keyFormatter: function (d) {
                    switch (d) {
                        case 'Q1':
                            return 'Leg1';

                        case 'Q2':
                            return 'Leg2';

                        case 'Q3':
                            return 'Leg3';

                        default:
                            return d;
                    }

                }
            },
            color: ['#31a354', '#de2d26', '#2b8cbe', '#756bb1', '#99d8c9'],
            x: function (d) { return d.label; },
            y: function (d) { return d.values.S1; },
            maxBoxWidth: 200,
            yDomain: [0, 1000]
        }
    };
    data: any = [];

    // dynamic chart loading
    chartTypes: any[] = [Nvd3BoxPlotWrapper];
    @ViewChild('chart', { read: ViewContainerRef }) chartContainer;

    constructor(
        private viewContainerRef: ViewContainerRef,
        private cfr: ComponentFactoryResolver,
        private resultService: ResultService,
        private sessionService: SessionService
    ) {
        this.viewOptions = [
            { id: 0, label: 'Avg. All' },
            { id: 1, label: 'Avg. Top 10' }
        ];
        this.selectedSession = this.sessionService.currentSession;
    }

    ngOnInit() {
        this.loadChartData();
    }

    autoRefreshChanged() {
        if (this.autoRefresh) {
            this.loadChartData();
            this.timerHandle = setInterval(() => {
                this.loadChartData();
            }, 5000);
        } else {
            clearInterval(this.timerHandle);
        }
    }

    updateSelectedView(event: string): void {
        this.loadChartData();
    }

    setYScaleMax() {
        this.options.chart.yDomain = [0, this.yScaleMax * 1000];
    }

    reset(e: any) {
        this.selectedSession = this.sessionService.currentSession;
    }

    loadChartData() {

        switch (this.selectedView) {
            case 0:
                this.data = [];
                break;
            case 1:
                this.resultService.getBoxPlotData(this.selectedSession).subscribe(r => {
                    console.log(r);
                    this.data = r;

                });
                break;
        }

        this.drawChart();
    }

    drawChart() {

        this.chartContainer.clear();
        const compFactory = this.cfr.resolveComponentFactory(this.chartTypes[0]);
        const component = this.chartContainer.createComponent(compFactory);
        const componentInstance = <Nvd3BoxPlotWrapper>component.instance;

        componentInstance.options = this.options;
        componentInstance.data = this.data;
    }

    ngOnDestroy() {
        // tslint:disable-next-line:no-unused-expression
        this.timerHandle && clearInterval(this.timerHandle);
    }
}