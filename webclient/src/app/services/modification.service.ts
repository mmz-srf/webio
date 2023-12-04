import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable, Subject} from 'rxjs';
import {
  QueryResult,
} from './entities';
import {PropertyChangeEvent} from './PropertyChangeEvent';
import {DeviceDto} from '../data/models/device-dto';
import {InterfaceDto} from '../data/models/interface-dto';
import {StreamDto} from '../data/models/stream-dto';
import {InterfaceTemplateSelectionDto} from '../data/models/interface-template-selection-dto';
import {DeviceUpdatedEventDto} from '../data/models/device-updated-event-dto';
import {PropertiesChangedSummaryDto} from '../data/models/properties-changed-summary-dto';
import {DeviceDeletedDto} from '../data/models/device-deleted-dto';
import {DeviceAddedEventDto} from '../data/models/device-added-event-dto';

@Injectable()
export class ModificationService {
  constructor(
    private http: HttpClient) {
    this.pendingSubject.next(0);
  }

  private modifications: PropertyChangeEvent[] = [];

  private pendingSubject = new Subject<number>();
  public pending$: Observable<number> = this.pendingSubject.asObservable();

  private dataChangedSubject = new Subject<boolean>();
  public dataChanged$: Observable<boolean> = this.dataChangedSubject.asObservable();
  public changeValue(
    property: string,
    newValue: string,
    entity: string,
    device: string,
    entityType: string) {

    // remove obsolete modifications
    this.modifications = this.modifications
      .filter(m => m.property !== property || m.entity !== entity);

    // add new modification
    const added = new PropertyChangeEvent(property, newValue, entity, device, entityType);
    this.modifications.push(added);
    this.pendingSubject.next(this.modifications.length);
  }

  public saveChanges(): Promise<any> {

    const args: PropertiesChangedSummaryDto = {
      changedEvents: this.modifications,
      comment: 'keiner'
    };

    const post = this.http.post('/api/changeEvents', args).toPromise();

    post.then(_ => {
      this.modifications = [];
      this.pendingSubject.next(this.modifications.length);
      this.dataChangedSubject.next(true);
    }).catch(e => {
      throw e;
    });

    return post;
  }

  public discardChanges() {
    this.modifications = [];
    this.pendingSubject.next(this.modifications.length);
    this.dataChangedSubject.next(true);
  }

  public applyDeviceModifications(result: QueryResult<DeviceDto>): QueryResult<DeviceDto> {
    this.modifications
      .filter(m => m.entityType === 'Device')
      .forEach(m => {
        result.data
          .filter(d => d.id === m.entity)
          .forEach(d => {
            d.properties[m.property].dirty = true;
            d.properties[m.property].value = m.newValue;
          });
      });

    return result;
  }

  public applyInterfaceModifications(result: QueryResult<InterfaceDto>): QueryResult<InterfaceDto> {
    this.modifications
      .filter(m => m.entityType === 'Interface')
      .forEach(m => {
        result.data
          .filter(d => d.id === m.entity)
          .forEach(d => {
            d.properties[m.property].dirty = true;
            d.properties[m.property].value = m.newValue;
          });
      });

    return result;
  }

  public applyStreamModifications(result: QueryResult<StreamDto>): QueryResult<StreamDto> {
    this.modifications
      .filter(m => m.entityType === 'Stream')
      .forEach(m => {
        result.data
          .filter(d => d.id === m.entity)
          .forEach(d => {
            d.properties[m.property].dirty = true;
            d.properties[m.property].value = m.newValue;
          });
      });

    return result;
  }

  public createDevice(
    name: string,
    deviceType: string,
    interfaces: InterfaceTemplateSelectionDto[],
    comment: string,
    useSt20227: boolean): Promise<void> {

    const args: DeviceAddedEventDto = {
      name,
      deviceType,
      interfaces,
      comment,
      useSt2022_7: useSt20227
    };

    const post = this.http.post<void>('/api/changeEvents/createDevice', args).toPromise();
    post.then(_ => {
      this.dataChangedSubject.next(true);
    });
    return post;
  }


  public updateDevice(
    deviceId: string,
    interfaces: InterfaceTemplateSelectionDto[],
    comment: string): Promise<void> {

    const args: DeviceUpdatedEventDto = {
      deviceId,
      interfaces,
      comment
    };

    const post = this.http.post<void>('/api/changeEvents/updateDevice', args).toPromise();
    post.then(_ => {
      this.dataChangedSubject.next(true);
    }).catch(e => {
      throw e;
    });
    return post;
  }

  public deleteDevice(deviceToDelete: DeviceDto, comment: string): Promise<void> {
    const args: DeviceDeletedDto = {
      deviceId: deviceToDelete.id,
      comment,
    };

    const post = this.http.post<void>('/api/changeEvents/deleteDevice', args).toPromise();
    post.then(_ => {
      this.dataChangedSubject.next(true);
    }).catch(e => {
      throw e;
    });
    return post;
  }
}
