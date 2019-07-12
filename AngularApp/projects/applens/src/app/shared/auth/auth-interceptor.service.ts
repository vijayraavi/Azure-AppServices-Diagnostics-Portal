import {Injectable} from '@angular/core';
import { Observable } from "rxjs";
import {HttpEvent, HttpInterceptor, HttpHandler, HttpRequest} from '@angular/common/http';
import { Router } from '@angular/router';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private _router: Router){}
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request)
        .do(event => {})
        .catch(err => {
            if (err.status == 401){
                this._router.navigate(['unauthorized']);
            }
            return Observable.throw(err);
        });
  }
}