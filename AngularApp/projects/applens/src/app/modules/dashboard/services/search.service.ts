import { Injectable } from '@angular/core';

@Injectable()
export class SearchService {
    public searchTerm: string = "";
    public searchId: string = "";
    public resourceHomeOpen: boolean = false;
    public newSearch: boolean = false;
}
