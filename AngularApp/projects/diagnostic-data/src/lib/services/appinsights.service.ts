import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable ,  BehaviorSubject, of, ReplaySubject } from 'rxjs';

@Injectable()
  
export class AppInsightsQueryService {

    CheckIfAppInsightsEnabled(): boolean
    {
        return false;
    }

    ExecuteQuerywithPostMethod(query: string): Observable<any> {
        return null;
    }
}

