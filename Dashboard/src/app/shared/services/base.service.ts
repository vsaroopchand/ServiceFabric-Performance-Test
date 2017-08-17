import { Http, Response, Headers } from '@angular/http';
import { Inject, Injectable } from '@angular/core';

import 'rxjs/Rx';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/throw';
import { Observer } from 'rxjs/Observer';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

import { BaseEndpoint } from './../../app.constants';

@Injectable()
export class BaseService<T> {
    headers: Headers;

    constructor(protected http: Http, @Inject(BaseEndpoint) protected baseApiEndpoint) {

        this.headers = new Headers({ 'Content-Type': 'application/json' });
        this.headers.append('Access-Control-Allow-Origin', 'http://localhost:4200');

    }

    getAll(): Observable<any> {
        return this.http.get(this.baseApiEndpoint, { headers: this.headers }).map(
            (res: Response) => {
                return res.json() as any[];
            }).catch(this.handleError);
    }

    get(id: number | string): Observable<any> {
        return this.http.get(this.baseApiEndpoint + '/' + id, { headers: this.headers }).map((value, i) => {
            return <T>value.json()
        })
            .catch(this.handleError);
    }

    protected handleError(error: any) {
        console.error('server error:', error);
        if (error instanceof Response) {
            let errMessage = '';
            try {
                errMessage = error.json().error;
            } catch (err) {
                errMessage = error.statusText;
            }
            return Observable.throw(errMessage);
        }
        return Observable.throw(error || 'server error');
    }
}