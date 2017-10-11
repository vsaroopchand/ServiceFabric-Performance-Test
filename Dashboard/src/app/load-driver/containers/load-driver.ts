import { EventHubCommunicationService } from './../../shared/services/eventhub-comm.service';
import { SessionService } from './../../shared/services/session.service';
import { RemotingCommunicationService } from './../../shared/services/remoting-comm.service';
import { SocketCommunicationService } from './../../shared/services/socket-comm.service';
import { WcfCommunicationService } from './../../shared/services/wcf-comm.service';
import { ServiceBusCommunicationService } from './../../shared/services/servicebus-comm.service';

import { Observable } from 'rxjs/Observable';
import { Component, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'my-load-driver-container',
    template: `
        <my-load-driver [form]="form" [inProgress]="inProgress" (onStartCallback)="start(e)" (onResetCallback)="reset(e)"></my-load-driver>
    `
})
export class LoadDriverContainer implements OnInit {

    form: FormGroup;
    @Output('onResetCallback') onResetCallback: EventEmitter<any> = new EventEmitter<any>();
    inProgress = false;

    constructor(private fb: FormBuilder,
        private wcf: WcfCommunicationService,
        private socket: SocketCommunicationService,
        private remoting: RemotingCommunicationService,
        private serviceBus: ServiceBusCommunicationService,
        private eventHub: EventHubCommunicationService,
        private sessionService: SessionService) {
    }

    ngOnInit() {
        this.form = this.fb.group({
            requests: this.fb.control(10, [Validators.min(1), Validators.max(1000)])
        });
    }

    reset(e: any) {
        this.sessionService.createSession();

        // notify any listeners
        this.onResetCallback && this.onResetCallback.emit({});
    }

    start(e: any) {
        let requestCount = +this.form.get('requests').value;
        const range = Observable.range(1, requestCount);
        const currentSession = this.sessionService.currentSession;

        this.inProgress = true;

        const source = Observable.from(range)
            .flatMap(function (item) {
                return Observable.of(item);
            }).throttle((t) => {
                return Observable.of(100);
            });

        source.subscribe(
            (x) => {

                Observable.merge(
                        this.wcf.get(currentSession),
                        this.remoting.get(currentSession),
                        this.socket.get(currentSession),
                        this.serviceBus.get(currentSession),
                        this.eventHub.get(currentSession),
                    4)
                    .subscribe(res => {
                        // do nothing
                    },
                    (error) => {
                        console.log('error');
                        console.log(error);
                    },
                    () => {
                        requestCount--;
                        if (requestCount === 0) {
                            this.inProgress = false;
                        }
                    });
            });
    }
}