import {Injectable} from '@angular/core';
import {EditDeviceComponent} from '../edit-device/edit-device.component';
import {EditStreamsComponent} from '../edit-streams/edit-streams.component';
import {GridOptions} from 'ag-grid-community';
import {EditInterfaceComponent} from '../edit-interface/edit-interface.component';

@Injectable()
export class DialogService {
  public editDevice: EditDeviceComponent;
  public editStream: EditStreamsComponent;
  public editInterface: EditInterfaceComponent;
  public gridOptions: GridOptions;
}
