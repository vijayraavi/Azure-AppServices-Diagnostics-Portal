import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GenericSolutionService {

  ArmApi(resourceUri: string, actionOptions: {}): Observable<any> {
    console.log(`Called ARM API with uri ${resourceUri} and options ${actionOptions['verb']}, ${actionOptions['route']}`);
    return of("Not implemented");
  }

  OpenTab(resourceUri: string, actionOptions: {}): Observable<any> {
    console.log("Called Open Tab");
    return of("Not implemented");
  }

  GoToBlade(resourceUri: string, actionOptions: {}): Observable<any> {
    console.log("Called Go To Blade");
    return of("Not implemented");
  }

}
