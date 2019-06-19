import { Injectable } from '@angular/core';
import { Http, Response, Headers, RequestOptions } from '@angular/http';

@Injectable()
export class SearchService {
    public appSettings: any = null;
    public searchIsEnabled: boolean = false;
    
    public searchTerm: string = "";
    public searchId: string = "";
    public resourceHomeOpen: boolean = false;
    public newSearch: boolean = false;
    
    constructor(private _http: Http){
        this._http.get('assets/appsettings.json').subscribe(res => {
            this.appSettings = res.json();
            this.searchIsEnabled = this.appSettings["SearchIsEnabled"];
        });
    }

    ngOnInit(){
    }
}
