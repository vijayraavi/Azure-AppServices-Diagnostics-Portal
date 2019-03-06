import { ActivatedRoute, Router, NavigationExtras } from '@angular/router';
import { TelemetryService } from './../../services/telemetry/telemetry.service';
import { Component, Inject } from '@angular/core';
import { DataRenderBaseComponent } from '../data-render-base/data-render-base.component';
import { DiagnosticService } from '../../services/diagnostic.service';
import { DataTableResponseColumn, DataTableResponseObject, DiagnosticData, RenderingType, Rendering} from '../../models/detector';
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
  appInsightDataList: AppInsightData[] = [];
  diagnosticDataSet: DiagnosticData[] = [];
  loadingAppInsightsData: boolean = true;
  appiicationInsightsDisabledInsight: DiagnosticData;

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
      });

    }
    else
    {
        //console.log("Not worrying about ai enabled");
    }

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
        renderingType: row[4]
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
          this.appInsightQueryMetaDataList.forEach(appInsightData => {
        
          this._appInsightsService.ExecuteQuerywithPostMethod(appInsightData.query).subscribe(data => {
            console.log(`Inside ExecuteQuerywithPostMethod, query: ${appInsightData.query}`);
            if (data && data["Tables"]) {
              console.log(`Table with query: ${appInsightData.query}`);
              console.log(data["Tables"]);
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
                  renderingType: appInsightData.renderingType,
                  table: rows,
                  poralBladeInfo: appInsightData.poralBladeInfo,
              });
              
              this.diagnosticDataSet.push(<DiagnosticData>{
                table: <DataTableResponseObject> {
                  columns: dataColumns,
                  rows: rows,
                },
                renderingProperties: <Rendering> {
                  type: appInsightData.renderingType,
                  title: appInsightData.title,
                  description: appInsightData.description
                }
              });
              console.log("rows and columns*********");
              console.log(rows);
              console.log(columns);

              console.log(appInsightData.renderingType);
              console.log("end*********");
            }

            console.log("dataset*********");
            console.log(this.diagnosticDataSet);
            
            
            this.loadingAppInsightsData = false;
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
  renderingType: any;
}

export class AppInsightData {
  title: string;
  description: string;
  table: any;
  poralBladeInfo: any;
  renderingType: any;
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