import { Component, OnInit } from '@angular/core';
import { DetectorItem } from '../category-page/category-page.component';
import { Router, ActivatedRoute, NavigationExtras} from '@angular/router';
import { DetectorMetaData } from 'diagnostic-data';
import { ApplensDiagnosticService } from '../services/applens-diagnostic.service';
import { ApplensSupportTopicService } from '../services/applens-support-topic.service';

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

  searchTerm: string = "";
  searchTermErrorDisplay: boolean = false;

  userImages: { [name: string]: string };

  authors: any[] = [];
  authorsList: string[] = [];

  constructor(private _route: Router, private _activatedRoute: ActivatedRoute, private _diagnosticService: ApplensDiagnosticService, private _supportTopicService: ApplensSupportTopicService) {
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

  triggerSearch(){
    if (this.searchTerm && this.searchTerm.length>3){
      console.log(this.searchTerm);
      this.navigateTo(`../search`, {searchTerm: this.searchTerm}, 'merge');
    }
  }

  executeSearch(searchTerm){
    this.filteredDetectorsLoaded = false;
    this._diagnosticService.getDetectors(true, searchTerm).subscribe((detectors: DetectorMetaData[]) => {
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
      this.filteredDetectorsLoaded = true;
      
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

              let detectorItem = new DetectorItem(detector.name, detector.description, iconString, detector.author, [], detectorUsersImages, [], onClick);
              this.detectors.push(detectorItem);

            });
          });
        });
      }
    });
  }

  ngOnInit() {
    this._activatedRoute.queryParams.subscribe(params => {
      var searchTerm = params['searchTerm'];
      this.searchTerm = searchTerm;
      if (this.searchTerm && this.searchTerm.length>3){
        this.searchTermErrorDisplay = false;
        this.executeSearch(searchTerm);
      }
      else{
        this.filteredDetectorsLoaded = true;
        this.displaySearchTermError();
      }
    });
  }

  displaySearchTermError(){
    this.searchTermErrorDisplay = true;
  }

  navigateToUserPage(userId: string) {
    this.navigateTo(`../../users/${userId}`);
  }
}
