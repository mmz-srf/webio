import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {QueryResult} from './entities';
import {IEntityService} from './device.service';
import {SearchService} from './search.service';
import {ModificationService} from './modification.service';
import {map} from 'rxjs/operators';
import {InterfaceDto} from '../data/models/interface-dto';
import {InterfacesApi} from '../data/services/interfaces-api';
import {DataFieldDto} from '../data/models/data-field-dto';
import {mapToFilter} from '../common/utils';

@Injectable()
export class InterfaceService implements IEntityService<InterfaceDto> {

  constructor(
    private interfaceApi: InterfacesApi,
    private search: SearchService,
    private modificationService: ModificationService) {
  }

  getInterfaces(
    start: number,
    count: number,
    sort: string,
    sortOrder: string,
    filter: Map<string, string>): Observable<QueryResult<InterfaceDto>> {

    const params: any = {
      start,
      count,
      sort,
      sortOrder: sortOrder ? sortOrder : 'desc',
      body: mapToFilter(filter),
    };

    if (this.search.globalSearchQuery) {
      params.global = this.search.globalSearchQuery;
    }

    return this.interfaceApi.get(params)
      .pipe(map(x => this.modificationService.applyInterfaceModifications(x)));
  }

  getEntities(start: number, count: number, sort: string, sortOrder: string,
              filter: Map<string, string>): Observable<QueryResult<InterfaceDto>> {
    return this.getInterfaces(start, count, sort, sortOrder, filter);
  }

  getFields(): Observable<DataFieldDto[]> {
    return this.interfaceApi.getFields();
  }
}
