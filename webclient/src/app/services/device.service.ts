import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {ModificationService} from './modification.service';
import {QueryResult} from './entities';
import {SearchService} from './search.service';
import {DevicesApi} from '../data/services/devices-api';
import {DeviceDtoQueryResultDto} from '../data/models/device-dto-query-result-dto';
import {DeviceDto} from '../data/models/device-dto';
import {DataFieldDto} from '../data/models/data-field-dto';
import {DeviceTypeDto} from '../data/models/device-type-dto';
import {DeviceDetailsDto} from '../data/models/device-details-dto';
import {mapToFilter} from '../common/utils';

export interface IEntityService<T> {
  getFields(): Observable<DataFieldDto[]>;

  getEntities(start: number, count: number, sort: string, sortOrder: string, filter: Map<string, string>):
    Observable<QueryResult<T>>;
}

@Injectable()
export class DeviceService implements IEntityService<DeviceDto> {
  constructor(
    private deviceApi: DevicesApi,
    private search: SearchService,
    private modificationService: ModificationService) {
  }

  getDevices(start: number, count: number, sort: string, sortOrder: string, filter: Map<string, string>):
    Observable<DeviceDtoQueryResultDto> {
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

    return this.deviceApi.get(params)
      .pipe(map(x => this.modificationService.applyDeviceModifications(x)));
  }

  getEntities(start: number, count: number, sort: string, sortOrder: string, filter: Map<string, string>):
    Observable<QueryResult<DeviceDto>> {
    return this.getDevices(start, count, sort, sortOrder, filter);
  }

  getFields(): Observable<DataFieldDto[]> {
    return this.deviceApi.getFields();
  }

  getDeviceTypes(): Observable<DeviceTypeDto[]> {
    return this.deviceApi.getTypes();
  }

  getDeviceDetails(deviceId: string): Observable<DeviceDetailsDto> {
    return this.deviceApi.getDeviceDetails({deviceId});
  }

  isDuplicateDeviceName(deviceName: string, ownId?: string): Observable<boolean> {
    return this.deviceApi.isDuplicate({deviceName, ownId});
  }
}
