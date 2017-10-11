import { Observable } from 'rxjs/Observable';
import { Http, Response } from '@angular/http';
import { BaseEndpoint } from './../../app.constants';
import { BaseService } from './base.service';
import { Injectable, Inject } from '@angular/core';

@Injectable()
export class EventHubCommunicationService extends BaseService<any>{
    constructor(http: Http, @Inject(BaseEndpoint) baseApiEndpoint) {
        super(http, baseApiEndpoint + '/api/eventhub');
    }

    get(id: string): Observable<any> {
        return this.http.get(this.baseApiEndpoint + '/' + id, { headers: this.headers }).map(
            (res: Response) => {
                return res.json() as any;
            }).catch(this.handleError);
    }
}