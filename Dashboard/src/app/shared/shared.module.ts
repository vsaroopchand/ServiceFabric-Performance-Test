import { EventHubCommunicationService } from './services/eventhub-comm.service';
import { ServiceBusCommunicationService } from './services/servicebus-comm.service';
import { SessionService } from './services/session.service';
import { RemotingCommunicationService } from './services/remoting-comm.service';
import { SocketCommunicationService } from './services/socket-comm.service';
import { WcfCommunicationService } from './services/wcf-comm.service';
import { ResultService } from './services/result.service';
import { NgModule } from '@angular/core';


@NgModule({
    providers: [
        ResultService,
        WcfCommunicationService,
        SocketCommunicationService,
        RemotingCommunicationService,
        ServiceBusCommunicationService,
        EventHubCommunicationService,
        SessionService
    ]
})
export class SharedServicesModule {
}