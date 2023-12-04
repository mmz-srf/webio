import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {QueryResult} from './entities';
import {IEntityService} from './device.service';
import {SearchService} from './search.service';
import {ModificationService} from './modification.service';
import {map} from 'rxjs/operators';
import {StreamsApi} from '../data/services/streams-api';
import {StreamDto} from '../data/models/stream-dto';
import {DataFieldDto} from '../data/models/data-field-dto';
import {TagDto} from '../data/models/tag-dto';
import {mapToFilter} from '../common/utils';

@Injectable()
export class StreamService implements IEntityService<StreamDto> {

  constructor(
    private streamApi: StreamsApi,
    private search: SearchService,
    private modificationService: ModificationService) {
  }

  getStreams(start: number, count: number, sort: string, sortOrder: string, filter: Map<string, string>):
    Observable<QueryResult<StreamDto>> {
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

    return this.streamApi.get(params)
      .pipe(map(x => this.modificationService.applyStreamModifications(x)));
  }

  getEntities(start: number, count: number, sort: string, sortOrder: string, filter: Map<string, string>):
    Observable<QueryResult<StreamDto>> {
    return this.getStreams(start, count, sort, sortOrder, filter);
  }

  getFields(): Observable<DataFieldDto[]> {
    return this.streamApi.getFields();
  }

  getTags(): Observable<TagDto[]> {
    return this.streamApi.getTags();
  }
}
