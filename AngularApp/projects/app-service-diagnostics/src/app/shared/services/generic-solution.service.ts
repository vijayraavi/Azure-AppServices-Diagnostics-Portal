import { TelemetryService } from 'diagnostic-data';
import { Observable, of } from 'rxjs';
import { Injectable } from '@angular/core';
import { DefaultUrlSerializer } from '@angular/router';
import { PortalService } from '../../startup/services/portal.service';
import { OpenBladeInfo } from '../models/portal';
import { ArmService } from './arm.service';

@Injectable({
  providedIn: 'root'
})
export class GenericSolutionService {

  constructor(private armService: ArmService, private portalService: PortalService, private logService: TelemetryService) {}

  validateArmApiOptions(options: {}) {
    // Check for keys: route, verb, body (optional)
    // Check for valid verb
    // Check for whitelisted route
  }

  buildRoute(resourceUri: string, routeSegment: string): string {
    return resourceUri + routeSegment;
  }

  getApiVersion(route: string): string | null {
    let urlTree = new DefaultUrlSerializer().parse(route);

    return 'api-version' in urlTree.queryParams ? urlTree.queryParams['api-version'] : null;
  }

  ArmApi(resourceUri: string, actionOptions: {}): Observable<any> {
    this.validateArmApiOptions(actionOptions);

    const verb = actionOptions['verb'].toLowerCase();
    const route = this.buildRoute(resourceUri, actionOptions['route']);
    const apiVersion = this.getApiVersion(route);
    const body = 'body' in actionOptions ? actionOptions['body'] : null;

    this.logService.logEvent('solution_arm_api', {'fullRoute': route, ...actionOptions});

    if (verb === 'get') {
      return this.armService.getResourceFullResponse(route, true, apiVersion);
    }

    let actionMethod = `${verb}ResourceFullResponse`;
    return this.armService[actionMethod](route, body, true, apiVersion);
  }

  OpenTab(resourceUri: string, actionOptions: {}): Observable<any> {
    this.logService.logEvent('solution_open_tab', {'resourceUri': resourceUri, ...actionOptions});
    return of("Not implemented");
  }

  getBladeInfo(options: {}) {
    if (!('detailBlade' in options)) {
      throw new Error('ActionOptions should include the detailBlade property');
    }
    return options as OpenBladeInfo;
  }

  GoToBlade(resourceUri: string, actionOptions: {}): Observable<any> {
    this.logService.logEvent('solution_goto_blade', {'resourceUri': resourceUri, ...actionOptions});

    const bladeInfo = this.getBladeInfo(actionOptions);

    this.portalService.openBlade(bladeInfo, 'troubleshoot');

    return of(bladeInfo.detailBlade);
  }

}
