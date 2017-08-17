import { Injectable } from '@angular/core';

@Injectable()
export class SessionService {

    private _sessions: Array<string> = [];

    constructor() {
        let session = this.uuidv4();
        this._sessions.push(session);
    }

    createSession() {
        let session = this.uuidv4();
        this._sessions.push(session);
        return session;
    }

    get sessions() {
        return this._sessions;
    }

    get currentSession() {
        let lastSessionId = this._sessions[this._sessions.length - 1];
        return lastSessionId;
    }

    uuidv4() {
        // https://stackoverflow.com/questions/105034/create-guid-uuid-in-javascript
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
}