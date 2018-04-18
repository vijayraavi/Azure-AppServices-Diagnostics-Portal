import { Http, Headers, Response, Request } from '@angular/http';
import { Injectable, EventEmitter } from '@angular/core';
import { Subscription } from '../models/subscription';
import { Site, SiteInfoMetaData } from '../models/site';
import { ArmObj } from '../models/armObj';
import { SiteConfig } from '../models/site-config';
import { ResponseMessageEnvelope, ResponseMessageCollectionEnvelope } from '../models/responsemessageenvelope'
import { Observable, Subscription as RxSubscription, Subject, ReplaySubject } from 'rxjs/Rx';
import { ResourceGroup } from '../models/resource-group';
import { PublishingCredentials } from '../models/publishing-credentials';
import { DeploymentLocations } from '../models/arm/locations';
import { AuthService } from './auth.service';
import { CacheService } from './cache.service';
import { ArmService } from './arm.service';
import { SiteService } from './site.service';

import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/throw';
import { DetectorResponse } from 'applens-diagnostics/src/app/diagnostic-data/models/detector';

@Injectable()
export class GenericApiService {
    private localEndpoint = "http://localhost:5000";

    siteInfo: SiteInfoMetaData;

    constructor(private _http: Http, private _authService: AuthService, private _cache: CacheService, private _armService: ArmService, private _siteService: SiteService) {
        this._siteService.currentSiteMetaData.subscribe(siteMetaData => {
            this.siteInfo = siteMetaData;
        });
    }

    public getDetectors() {
        let resourceUri = this.siteInfo.resourceUri.replace('resourcegroups', 'resourceGroups');
        let path = `v4${resourceUri}/detectors?stampName=waws-prod-bay-085&hostnames=netpractice.azurewebsites.net`;
        return this.invoke<DetectorResponse>(path, 'POST');
    }

    public getDetector(detectorName: string) {
        let path = `v4${this.siteInfo.resourceUri}/detectors/${detectorName}?stampName=waws-prod-bay-085&hostnames=netpractice.azurewebsites.net`;
        return this.invoke<DetectorResponse>(path, 'POST');
    }

    public invoke<T>(path: string, method = 'GET', body: any = {}): Observable<T> {
        var url: string = `${this.localEndpoint}/api/invoke`

        let request = this._http.post(url, body, {
            headers: this._getHeaders(path, method)
        })
        .map((response: Response) => <T>(response.json()));

        return request;
    }

    private _getHeaders(path?: string, method?: string): Headers {
        var headers = new Headers();
        headers.append('Content-Type', 'application/json');
        headers.append('Accept', 'application/json');
    
        if (path) {
          headers.append('x-ms-path-query', path);
        }
    
        if (method) {
          headers.append('x-ms-method', method);
        }
    
        return headers;
      }

}