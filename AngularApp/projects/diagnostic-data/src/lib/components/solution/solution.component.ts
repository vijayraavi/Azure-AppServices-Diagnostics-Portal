import { Observable } from 'rxjs';
import { Component, Input } from '@angular/core';
import { Dictionary } from '../../../../../applens/src/app/shared/models/extensions';
import { Rendering } from '../../models/detector';
import { TelemetryService } from '../../services/telemetry/telemetry.service';
import { DataRenderBaseComponent } from '../data-render-base/data-render-base.component';
import { UriUtilities } from '../../utilities/uri-utilities';
import { SolutionService } from '../../services/solution.service';
import { SolutionTypeTag } from '../../models/solution-type-tag';

export enum ActionType {
  ArmApi = 'ArmApi',
  OpenTab = 'OpenTab',
  GoToBlade = 'GoToBlade'
}

export class Solution {
  Name: string;
  Title: string;
  Description: string;
  Action: ActionType;
  RequiresConfirmation: boolean;
  ResourceUri: string;
  InternalInstructions: string;
  ActionOptions: Dictionary<any>;
  TypeTag: SolutionTypeTag;
  IsInternal: boolean;
  DetectorId: string;
}

@Component({
  selector: 'solution',
  templateUrl: './solution.component.html',
  styleUrls: ['./solution.component.scss']
})
export class SolutionComponent extends DataRenderBaseComponent {

  @Input("data") solution: Solution;
  renderingProperties: Rendering;
  actionStatus: string;
  defaultCopyText = 'Copy';
  copyText = this.defaultCopyText;
  appName: string;

  constructor(telemetryService: TelemetryService, private _siteService: SolutionService) {
    super(telemetryService);
  }

  ngOnInit() {
    let uriParts = this.solution.ResourceUri.split('/');
    this.appName = uriParts[uriParts.length - 1];

    this.buildSolutionText();
  }

  buildSolutionText() {
    let detectorLink = UriUtilities.BuildDetectorLink(this.solution.ResourceUri, this.solution.DetectorId);
    let detectorLinkMarkdown = `[Go To Detector](${detectorLink})`;
    this.solution.InternalInstructions = detectorLinkMarkdown + "\n\n" + this.solution.InternalInstructions;
  }

  performAction() {
    this.actionStatus = "Running...";

    this.chooseAction(this.solution.Action, this.solution.ResourceUri, this.solution.ActionOptions).subscribe(res => {
      if (res.ok == null || res.ok) {
        this.actionStatus = "Complete!"
      } else {
        this.actionStatus = `Error completing request. Status code: ${res.status}`
      }
    });
  }

  chooseAction(actionType: ActionType, resourceUri: string, args?: Dictionary<string>): Observable<any> {
    const actionName = actionType.toString();

    if (!this._siteService[actionName]) {
      throw new Error(`Method Not Found: Solution API does not have a method named ${actionName}`)
    }

    console.log(args['verb']);
    return this._siteService[actionName](resourceUri, args);
  }

  copyInstructions(copyValue: string) {
    this.copyText = "Copying..";

    let selBox = document.createElement('textarea');
    selBox.style.position = 'fixed';
    selBox.style.left = '0';
    selBox.style.top = '0';
    selBox.style.opacity = '0';
    selBox.value = copyValue;
    document.body.appendChild(selBox);
    selBox.focus();
    selBox.select();
    document.execCommand('copy');
    document.body.removeChild(selBox);

    this.copyText = "Copied!";

    setTimeout(() => {
      this.copyText = this.defaultCopyText;
    }, 2000);
  }

}
