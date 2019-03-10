import { ActivatedRoute, Router, NavigationExtras } from '@angular/router';
import { TelemetryService } from './../../services/telemetry/telemetry.service';
import { Component, Inject } from '@angular/core';
import { DataRenderBaseComponent } from '../data-render-base/data-render-base.component';
import { DiagnosticService } from '../../services/diagnostic.service';
import { DataTableResponseColumn, DataTableResponseObject, DiagnosticData, RenderingType, Rendering, TimeSeriesType, TimeSeriesRendering} from '../../models/detector';
import { FeatureNavigationService } from '../../services/feature-navigation.service';
import { DIAGNOSTIC_DATA_CONFIG, DiagnosticDataConfig } from '../../config/diagnostic-data-config';
import { AppInsightsQueryService } from '../../services/appinsights.service';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'app-insights-markdown',
  templateUrl: './app-insights-markdown.component.html',
  styleUrls: ['./app-insights-markdown.component.scss'],
  animations: [
    trigger(
        'loadingAnimation',
        [
            state('shown', style({
                opacity: 1
            })),
            state('hidden', style({
                opacity: 0
            })),
            transition('* => *', animate('.5s'))
        ]
    )
]
})

export class AppInsightsMarkdownComponent extends DataRenderBaseComponent {

  renderingProperties: Rendering;
  isPublic: boolean;
  isAppInsightsEnabled: boolean = false;
  appInsightQueryMetaDataList: AppInsightQueryMetadata[] = [];
  appInsightDataList: AppInsightData[] = [];
  diagnosticDataSet: DiagnosticData[] = [];
  loadingAppInsightsResource: boolean = true;
  loadingAppInsightsQueryData: boolean = true;

  constructor(private _appInsightsService: AppInsightsQueryService, private _diagnosticService: DiagnosticService, private _router: Router,
    private _activatedRoute: ActivatedRoute, protected telemetryService: TelemetryService, private _navigator: FeatureNavigationService, @Inject(DIAGNOSTIC_DATA_CONFIG) config: DiagnosticDataConfig) {
    super(telemetryService);
    this.isPublic = config && config.isPublic;
    // this.appiicationInsightsDisabledInsight = <DiagnosticData>{
    //   table: <DataTableResponseObject> {
    //     columns: dataColumns,
    //     rows: rows,
    //   },
    //   renderingProperties: <Rendering> {
    //     type: RenderingType.Insights,
    //     title: "",
    //     description: ""
    //   };

    if (this.isPublic)
    {
      this._appInsightsService.CheckIfAppInsightsEnabled().subscribe(isAppinsightsEnabled => {
        this.isAppInsightsEnabled = isAppinsightsEnabled;
        this.loadingAppInsightsResource = false;

       
      });

    }
    else
    {
        //console.log("Not worrying about ai enabled");
    }

  }

  // title: string;
  // description: string;
  // query: string;
  // poralBladeInfo: any;
  // renderingProperties: any;

//   startLoadingMessage(): void {
//     let self = this;
//     this.loadingMessageIndex = 0;
//     this.showLoadingMessage = true;

//     setTimeout(() => {
//         self.showLoadingMessage = false;
//     }, 3000)
//     this.loadingMessageTimer = setInterval(() => {
//         self.loadingMessageIndex++;
//         self.showLoadingMessage = true;

//         if (self.loadingMessageIndex === self.loadingMessages.length - 1) {
//             clearInterval(this.loadingMessageTimer);
//             return;
//         }

//         setTimeout(() => {
//             self.showLoadingMessage = false;
//         }, 3000)
//     }, 4000);
// }

  public getMetaDataMarkdown(metaData: AppInsightQueryMetadata)
  {
      // let str = "<h1>"+ metaData.title + "</h1>";
      // str += "<h4>"+ metaData.description + "</h4>";

     let str = "<p style='font-weight:bold'>Ask the customer to run the following queries in the Application Insights Analytics:</p>";
      str += "<pre>" + metaData.query + "</pre>";
      return str;
  }

  protected processData(data: DiagnosticData) {
    console.log("I am processsing I am processing");
    super.processData(data);
    this.renderingProperties = <Rendering>data.renderingProperties;

    data.table.rows.map(row => {
      this.appInsightQueryMetaDataList.push(<AppInsightQueryMetadata>{
        title: row[0],
        description: row[1],
        query: row[2],
        poralBladeInfo: row[3],
        renderingProperties: row[4]
        // icon: 'fa-bar-chart',
        // linkType: parseInt(row[3]),
        // linkValue: row[4]
      });
    });

    
    if (this.isPublic && this.appInsightQueryMetaDataList !== [])
    {
      
        this._appInsightsService.loadAppInsightsResourceObservable.subscribe(loadStatus => {
          if (loadStatus === true)
        {
          this.loadingAppInsightsResource = false;
          this.appInsightQueryMetaDataList.forEach(appInsightData => {
        
          this._appInsightsService.ExecuteQuerywithPostMethod(appInsightData.query).subscribe(data => {
            
            if (data && data["Tables"]) {
              let rows = data["Tables"][0]["Rows"];
              let columns = data["Tables"][0]["Columns"];
              let dataColumns: DataTableResponseColumn[] = [];
              columns.forEach(column => {
                dataColumns.push(<DataTableResponseColumn>{
                  columnName: column.ColumnName,
                  dataType: column.DataType,
                  columnType: column.ColumnType,
                }

                )
                
              });

              this.appInsightDataList.push(<AppInsightData>{
                  title: appInsightData.title,
                  description: appInsightData.description,
                  renderingProperties: appInsightData.renderingProperties,
                  table: rows,
                  poralBladeInfo: appInsightData.poralBladeInfo,
                  diagnosticData: <DiagnosticData>{
                    table: <DataTableResponseObject> {
                      columns: dataColumns,
                      rows: rows,
                    },
                    renderingProperties: appInsightData.renderingProperties,
                  }
              });
              
              // this.diagnosticDataSet.push(<DiagnosticData>{
              //   table: <DataTableResponseObject> {
              //     columns: dataColumns,
              //     rows: rows,
              //   },
              //   renderingProperties: appInsightData.renderingProperties,
              // });

              // this.diagnosticDataSet.push(<DiagnosticData>{
              //   table: <DataTableResponseObject> {
              //     columns: dataColumns,
              //     rows: rows,
              //   },

              // //   export interface TimeSeriesRendering extends Rendering {
              // //     defaultValue: number;
              // //     graphType: TimeSeriesType;
              // //     graphOptions: any;
              // //     timestampColumnName: string;
              // //     counterColumnName: string;
              // //     seriesColumns: string[];
              // // }

              // renderingProperties: appInsightData.renderingProperties,
              // });
            }
            
            this.loadingAppInsightsQueryData = false;
        });
      });
    }
 
    console.log(this.diagnosticDataSet);
  
      });
    }  
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
  renderingProperties: any;
}

export class AppInsightData {
  title: string;
  description: string;
  table: any;
  poralBladeInfo: any;
  renderingProperties: any;
  diagnosticData: DiagnosticData;
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