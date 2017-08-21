import { Observable } from 'rxjs/Observable';
import { BaseEndpoint } from './../../app.constants';
import { Http, Response } from '@angular/http';

import { BaseService } from './base.service';
import { Injectable, Inject } from '@angular/core';

@Injectable()
export class RemotingCommunicationService extends BaseService<any>{
    constructor(http: Http, @Inject(BaseEndpoint) baseApiEndpoint) {
        super(http, baseApiEndpoint + '/api/remoting');
    }

    get(id: string): Observable<any> {
        return this.http.get(this.baseApiEndpoint + '/start/' + id, { headers: this.headers }).map(
            (res: Response) => {
                return res.json() as any;
            }).catch(this.handleError);
    }
}