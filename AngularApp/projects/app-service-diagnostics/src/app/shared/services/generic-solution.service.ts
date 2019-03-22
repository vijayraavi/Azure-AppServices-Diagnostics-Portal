import { TelemetryService } from 'diagnostic-data';
import { Observable, of } from 'rxjs';
import { Injectable } from '@angular/core';
import { DefaultUrlSerializer, Router } from '@angular/router';
import { PortalService } from '../../startup/services/portal.service';
import { OpenBladeInfo } from '../models/portal';
import { ArmService } from './arm.service';
import { PortalActionService } from './portal-action.service';

@Injectable({
  providedIn: 'root'
})
export class GenericSolutionService {

  whitelistedRoutes = {
    'post': ['restart']
  }

  constructor(private armService: ArmService, private portalService: PortalService,
    private logService: TelemetryService, private portalNavService: PortalActionService,
    private _router: Router) {}

  assertPropertyExists(dict: {}, property: string) {
    if (!(property in dict)) {
      throw new Error(`Property Not Found: expected property named "${property}"`);
    }
  }

  validateArmApiOptions(options: {route: string, verb: string}) {
    for (let prop of ['route', 'verb']) {
      this.assertPropertyExists(options, prop);
    }

    let cleanedRoute = options.route.startsWith('/') ? options.route.substring(1) : options.route;

    if (!(this.whitelistedRoutes[options.verb.toLowerCase()].includes(cleanedRoute))) {
      throw new Error(`Invalid Operation: cannot perform ${options.verb} on route ${cleanedRoute}`)
    }
  }

  buildRoute(resourceUri: string, routeSegment: string): string {
    return resourceUri + routeSegment;
  }

  getApiVersion(route: string): string | null {
    let urlTree = new DefaultUrlSerializer().parse(route);

    return 'api-version' in urlTree.queryParams ? urlTree.queryParams['api-version'] : null;
  }

  ArmApi(resourceUri: string, actionOptions: {route: string, verb: string}): Observable<any> {
    this.validateArmApiOptions(actionOptions);

    const verb = actionOptions['verb'].toLowerCase();
    const route = this.buildRoute(resourceUri, actionOptions['route']);
    const apiVersion = this.getApiVersion(route);
    const body = 'body' in actionOptions ? actionOptions['body'] : null;

    this.logService.logEvent('SolutionArmApi', {'fullRoute': route, ...actionOptions});

    if (verb === 'get') {
      return this.armService.getResourceFullResponse(route, true, apiVersion);
    }

    let actionMethod = `${verb}ResourceFullResponse`;
    return this.armService[actionMethod](route, body, true, apiVersion);
  }

  OpenTab(resourceUri: string, actionOptions: {tabUrl: string}): Observable<any> {
    this.logService.logEvent('SolutionOpenTab', {'resourceUri': resourceUri, ...actionOptions});

    if (!('tabUrl' in actionOptions)) {
      throw new Error('ActionOptions should include the tabUrl property');
    }

    const tabUrl = actionOptions['tabUrl'].toLowerCase();
    this._router.navigateByUrl(tabUrl);

    return of(tabUrl);
  }

  getBladeInfo(options: {}) {
    if (!('detailBlade' in options)) {
      throw new Error('ActionOptions should include the detailBlade property');
    }

    return options as OpenBladeInfo;
  }

  GoToBlade(resourceUri: string, actionOptions: {detailBlade: string}): Observable<any> {
    this.logService.logEvent('SolutionGoToBlade', {'resourceUri': resourceUri, ...actionOptions});

    const bladeInfo = this.getBladeInfo(actionOptions);

    switch (bladeInfo.detailBlade) {
      case 'scaleup': {
        this.portalNavService.openBladeScaleUpBlade();
        break;
      }
      case 'AutoScaleSettingsBlade': {
        this.portalNavService.openBladeScaleOutBlade();
        break;
      }
      case 'TinfoilSecurityBlade': {
        this.portalNavService.openTifoilSecurityBlade();
        break;
      }
      default: {
        this.portalService.openBlade(bladeInfo, 'troubleshoot');
      }
    }

    return of(bladeInfo.detailBlade);
  }

}
