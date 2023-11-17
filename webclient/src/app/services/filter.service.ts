import {Injectable} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {GridApi} from 'ag-grid-community';
import {errorHappensBecauseColumnsArentLoaded} from '../common/ag-grid-helper';

@Injectable({
  providedIn: 'root'
})
export class FilterService {
  constructor(private route: ActivatedRoute) {
  }

  public apply(gridApi: GridApi) {
    gridApi.setFilterModel(null);
    const filterParams = this.route.snapshot.queryParamMap.keys;

    try {
      filterParams.map(filterName => this.toGridFilter(gridApi, filterName));
      gridApi.onFilterChanged();
    } catch (e) {
      if (!errorHappensBecauseColumnsArentLoaded(e)) {
        throw e;
      }
    }
  }

  private toGridFilter(gridApi: GridApi, filterName: string) {
    const filter = gridApi.getFilterInstance(this.capitalize(filterName));
    if (filter) {
      filter.setModel({type: 'contains', filter: this.route.snapshot.queryParamMap.get(filterName)});
    }
    return filter;
  }

  private capitalize(str: string): string {
    return str.charAt(0).toUpperCase() + str.slice(1);
  }
}
