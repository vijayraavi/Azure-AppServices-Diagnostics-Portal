import { ActivatedRoute, Router, NavigationExtras } from '@angular/router';
import { TelemetryService } from './../../services/telemetry/telemetry.service';
import { Component, Inject } from '@angular/core';
import { DataRenderBaseComponent } from '../data-render-base/data-render-base.component';
import { DiagnosticService } from '../../services/diagnostic.service';
import { DiagnosticData, RenderingType, Rendering} from '../../models/detector';
import { FeatureNavigationService } from '../../services/feature-navigation.service';
import { DIAGNOSTIC_DATA_CONFIG, DiagnosticDataConfig } from '../../config/diagnostic-data-config';
import { AppInsightsQueryService } from '../../services/appinsights.service';

@Component({
  selector: 'app-insights-markdown',
  templateUrl: './app-insights-markdown.component.html',
  styleUrls: ['./app-insights-markdown.component.scss']
})
export class AppInsightsMarkdownComponent extends DataRenderBaseComponent {

  renderingProperties: Rendering;
  isPublic: boolean;
  isAppInsightsEnabled: boolean = false;
  appInsightQueryMetaDataList: AppInsightQueryMetadata[] = [];

  constructor(private _appInsightsService: AppInsightsQueryService, private _diagnosticService: DiagnosticService, private _router: Router,
    private _activatedRoute: ActivatedRoute, protected telemetryService: TelemetryService, private _navigator: FeatureNavigationService, @Inject(DIAGNOSTIC_DATA_CONFIG) config: DiagnosticDataConfig) {
    super(telemetryService);
    this.isPublic = config && config.isPublic;

    this.isAppInsightsEnabled = this._appInsightsService.CheckIfAppInsightsEnabled();


      // if (changes['startTime'] && this.appInsightsService.appInsightsSettings.validForStack) {
      //     this.exceptions = [];
      //     this.loading = true;
      //     this.appInsightsService.loadAppInsightsResourceObservable.subscribe(loadStatus => {
      //         this.appInsightsQueryService.GetTopExceptions(this.startTime, this.endTime).subscribe((data: any) => {
      //             let rows = data["Tables"][0]["Rows"];
      //             this.parseRowsIntoExceptions(rows);
      //             this.loading = false;

      //             this.logger.LogAppInsightsExceptionSummary(this.startTime, this.endTime, this.exceptionTypes);
      //         });
      //     });
      // }


  }

  protected processData(data: DiagnosticData) {
    super.processData(data);
    this.renderingProperties = <Rendering>data.renderingProperties;

    data.table.rows.map(row => {
      this.appInsightQueryMetaDataList.push(<AppInsightQueryMetadata>{
        title: row[0],
        description: row[1],
        query: row[2],
        poralBladeInfo: row[3],
        // icon: 'fa-bar-chart',
        // linkType: parseInt(row[3]),
        // linkValue: row[4]
      });
      console.log(row[3].detailBlade);
      console.log(row[3].extension);
      console.log(row[3].detailBladeInputs);
    })
  }

  // protected processData(data: DiagnosticData) {
  //   super.processData(data);
  //   this.renderingProperties = <Rendering>data.renderingProperties;

  //   this.createViewModel();
  // }
}




// export class CardSelectionComponent extends DataRenderBaseComponent {

//   cardSelections: CardSelection[] = [];
//   colors: string[] = ['rgb(186, 211, 245)', 'rgb(249, 213, 180)', 'rgb(208, 228, 176)', 'rgb(208, 175, 239)', 'rgb(170, 192, 208)', 'rgb(208, 170, 193)', 'rgb(166, 216, 209)', 'rgb(207, 217, 246)'];

//   constructor(private _diagnosticService: DiagnosticService, private _router: Router,
//     private _activatedRoute: ActivatedRoute, protected telemetryService: TelemetryService, private _navigator: FeatureNavigationService) {
//     super(telemetryService);
//   }

//   protected processData(data: DiagnosticData) {
//     super.processData(data);

//     data.table.rows.map(row => {
//       this.cardSelections.push(<CardSelection>{
//         title: row[0],
//         descriptions: JSON.parse(row[2]),
//         icon: 'fa-bar-chart',
//         linkType: parseInt(row[3]),
//         linkValue: row[4]
//       });
//     })
//   }

//   public selectCard(card: CardSelection) {
//     if (card && card.linkType === CardActionType.Detector) {
//       this._navigator.NavigateToDetector(this._activatedRoute.snapshot.params['detector'], card.linkValue);
//     }
//   }

//   public getColor(index: number): string {
//     return this.colors[index % this.colors.length];
//   }
// }

export class AppInsightQueryMetadata {
  title: string;
  description: string;
  query: string;
  poralBladeInfo: any;
}

export class BladeInfo
{
    detailBlade: string;
    extension: string;
    detailBladeInputs: any;
}
// public class AppInsightsOperationContext
// {
//     public string Title;
//     public string Description;
//     public string Query;
//     public BladeInfo PortalBlade;
//     public RenderingType RenderingType;
// }

// export enum CardActionType {
//   Detector,
//   Tool
// }