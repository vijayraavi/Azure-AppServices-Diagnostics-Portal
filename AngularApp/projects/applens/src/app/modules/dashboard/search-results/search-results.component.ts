import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, NavigationExtras} from '@angular/router';
import { DetectorMetaData } from 'diagnostic-data';
import { ApplensDiagnosticService } from '../services/applens-diagnostic.service';
import { ApplensSupportTopicService } from '../services/applens-support-topic.service';
import { TelemetryService } from '../../../../../../diagnostic-data/src/lib/services/telemetry/telemetry.service';
import {TelemetryEventNames} from '../../../../../../diagnostic-data/src/lib/services/telemetry/telemetry.common';
import { Location } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { v4 as uuid } from 'uuid';
import { SearchService } from '../services/search.service';

@Component({
  selector: 'search-results',
  templateUrl: './search-results.component.html',
  styleUrls: ['./search-results.component.scss']
})
export class SearchResultsComponent implements OnInit {
  detectors: DetectorItem[] = [];
  filterdDetectors: DetectorMetaData[] = [];
  filteredDetectorsLoaded: boolean = false;
  filterdDetectorAuthors: string[] = [];

  searchResultsFetchError: string = "";
  searchResultsFetchErrorDisplay: boolean = false;

  searchTermErrorDisplay: boolean = false;

  userImages: { [name: string]: string };

  authors: any[] = [];
  authorsList: string[] = [];

  searchTermDisplay: string = "";

  constructor(private _telemetryService: TelemetryService, private _route: Router, private _activatedRoute: ActivatedRoute, private _diagnosticService: ApplensDiagnosticService, private _supportTopicService: ApplensSupportTopicService, private _location: Location, public _searchService: SearchService) {
  }

  navigateTo(path: string, queryParams?: any, queryParamsHandling?: any) {
    let navigationExtras: NavigationExtras = {
      queryParamsHandling: queryParamsHandling || 'preserve',
      preserveFragment: true,
      relativeTo: this._activatedRoute,
      queryParams: queryParams
    };
    this._route.navigate([path], navigationExtras);
  }

  navigateBack(){
    this._location.back();
  }

  triggerSearch(){
    if (this._searchService.searchTerm && this._searchService.searchTerm.length>3){
      this.navigateTo(`../search`, {searchTerm: this._searchService.searchTerm}, 'merge');
    }
  }

  executeSearch(searchTerm){
    this.filteredDetectorsLoaded = false;
    if (!this._searchService.searchId || this._searchService.searchId.length==0){
      this._searchService.searchId = uuid();
      this._searchService.newSearch = true;
    }
    this._diagnosticService.getDetectors(true, searchTerm).subscribe((detectors: DetectorMetaData[]) => {
      if (this._searchService.newSearch) {
        this._telemetryService.logEvent(TelemetryEventNames.SearchQueryResults, { searchId: this._searchService.searchId, query: searchTerm, results: JSON.stringify(detectors.map((det: DetectorMetaData) => new Object({ id: det.id, score: det.score}))), ts: Math.floor((new Date()).getTime() / 1000).toString() });
        this._searchService.newSearch = false;
      }
      // This is to get the full detectors authors list, and make graph API call
      let authorString = "";
      this.filterdDetectors = [];
      this.detectors = [];
      detectors.forEach(detector => {
          if (detector.author != undefined && detector.author !== '') {
              authorString = authorString + "," + detector.author;
          }
          this.filterdDetectors.push(detector);
      });
      this.searchTermDisplay = this._searchService.searchTerm.valueOf();
      setTimeout(() => {
        this.filteredDetectorsLoaded = true;
        setTimeout(() => {
          document.getElementById("search-result-0").focus();
          this._telemetryService.logEvent(TelemetryEventNames.SearchResultsLoaded, {"searchId": this._searchService.searchId, "ts": Math.floor((new Date()).getTime() / 1000).toString()});
        }, 100);
      }, 500);
      
      const separators = [' ', ',', ';', ':'];
      let authors = authorString.toLowerCase().split(new RegExp(separators.join('|'), 'g'));
      authors.forEach(author => {
          if (author && author.length > 0 && !this.authorsList.find(existingAuthor => existingAuthor === author)) {
              this.authorsList.push(author);
          }
      });
      var body = {
        authors: this.authorsList
      };

      if (detectors !== null){
        this._diagnosticService.getUsers(body).subscribe((userImages) => {
          this.userImages = userImages;

          this.filterdDetectors.forEach((detector) => {
            this._supportTopicService.getCategoryImage(detector.name).subscribe((iconString) => {
              let onClick = () => {
                this.navigateTo(`../detectors/${detector.id}`);
              };

              let detectorUsersImages: { [name: string]: string } = {};
              if (detector.author != undefined) {
                let authors = detector.author.toLowerCase();
                const separators = [' ', ',', ';', ':'];
                let detectorAuthors = authors.split(new RegExp(separators.join('|'), 'g')).filter(author=> author != '');
                detectorAuthors.forEach(author => {
                  if (!this.filterdDetectorAuthors.find(existingAuthor => existingAuthor === author)) {
                      this.filterdDetectorAuthors.push(author);
                  }
                  detectorUsersImages[author] = this.userImages.hasOwnProperty(author) ? this.userImages[author] : undefined;
                });
              }

              let detectorItem = new DetectorItem(detector.id, detector.name, detector.description, iconString, detector.author, [], detectorUsersImages, [], onClick, detector.score);
              this.detectors.push(detectorItem);

            });
          });
        });
      }
    },
    (err: HttpErrorResponse)=> {
      this.filteredDetectorsLoaded = true;
      this.searchResultsFetchError = "I am sorry, some error occurred while processing.";
      this.searchResultsFetchErrorDisplay = true;
    });
  }

  detectorClick(detector, index){
    // Log detector click and navigate the respective detector
    this._telemetryService.logEvent(TelemetryEventNames.SearchResultClicked, { searchId: this._searchService.searchId, detectorId: detector.id, rank: (index+1).toString(), ts: Math.floor((new Date()).getTime() / 1000).toString() });
    detector.onClick();
  }

  ngOnInit() {
    this._activatedRoute.queryParams.subscribe(params => {
      var searchTerm = params['searchTerm'];
      this._searchService.searchTerm = searchTerm;
      if (this._searchService.searchTerm && this._searchService.searchTerm.length>3){
        this.searchTermErrorDisplay = false;
        this.searchResultsFetchErrorDisplay = false;
        this.executeSearch(searchTerm);
      }
      else{
        this.filteredDetectorsLoaded = true;
        this.displaySearchTermError();
      }
    });
  }

  searchResultFeedback(detector, rating){
    this._telemetryService.logEvent(TelemetryEventNames.SearchResultFeedback, {"detectorId": detector.id, "rating": rating});
  }

  displaySearchTermError(){
    this.searchTermErrorDisplay = true;
  }

  navigateToUserPage(userId: string) {
    this.navigateTo(`../../users/${userId}`);
  }
}

class DetectorItem {
  id: string;
  name: string;
  description: string;
  icon: string;
  authorString: string;
  authors: any[] = [];
  userImages: any;
  supportTopics: any[] = [];
  score: number;
  onClick: Function;

  constructor(id: string, name: string, description: string, icon: string, authorString: string, authors: any[], userImages: any, supportTopics: any[], onClick: Function, score: number) {
      this.name = name;
      this.id = id;

      if (description == undefined || description === "") {
          description = "This detector doesn't have any description."
      }
      this.description = description;
      this.icon = icon;
      this.authorString = authorString;
      this.authors = authors;
      this.userImages = userImages;
      this.supportTopics = supportTopics;
      this.onClick = onClick;
      this.score = score;
  }
}
