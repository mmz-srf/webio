import {Injectable} from '@angular/core';
import {Subject} from 'rxjs';

@Injectable()
export class SearchService {

  constructor() {
  }

  public globalSearchQueryChanged$: Subject<string> = new Subject<string>();
  public globalSearchQuery: string;

  public setGlobalSearch(query: string): void {
    this.globalSearchQuery = query;
    this.globalSearchQueryChanged$.next(query);
  }
}
