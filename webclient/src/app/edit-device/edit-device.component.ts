import {Component, OnInit} from '@angular/core';
import {Router} from '@angular/router';
// import {ConfirmBoxInitializer, DialogLayoutDisplay} from '@costlydeveloper/ngx-awesome-popup';
import {DeviceService} from '../services/device.service';
import {DialogService} from '../services/dialog.service';
import {ModificationService} from '../services/modification.service';
import {ApiDataService} from '../services/api-data.service';
import {takeUntil} from 'rxjs/operators';
import {DestructibleComponent} from '../common/destructible-component';
import {InterfaceTemplateDto} from '../data/models/interface-template-dto';
import {InterfaceTemplateSelectionDto} from '../data/models/interface-template-selection-dto';
import {DeviceDto, DeviceTypeDto} from '../data/models';
import {ConfirmBoxInitializer, DialogLayoutDisplay} from '@costlydeveloper/ngx-awesome-popup';
import {FormsModule} from '@angular/forms';
import {NgForOf, NgIf} from '@angular/common';


export class InterfaceTemplateSelection {
  constructor(
    public templateName: string,
    public audioSend: number,
    public audioReceive: number,
    public videoSend: number,
    public videoReceive: number,
    public ancillarySend: number,
    public ancillaryReceive: number) {
  }
}

@Component({
  selector: 'app-edit-device',
  templateUrl: './edit-device.component.html',
  styleUrls: ['./edit-device.component.scss'],
  imports: [
    FormsModule,
    NgForOf,
    NgIf
  ],
  standalone: true
})
export class EditDeviceComponent extends DestructibleComponent implements OnInit {

  public devices: DeviceDto[];
  public deviceToEdit: DeviceDto;
  public comment;

  public deviceType: DeviceTypeDto;
  public availableInterfaceTemplates: InterfaceTemplateDto[] = [];

  public interfaceCount = 0;
  public interfaceTemplates: InterfaceTemplateSelectionDto[];
  public interfaceEditBlockSize = 1;
  public interfaceNames = [];
  public interfaceList = [];
  public deviceName;
  public interfaceName;


  public allowModifyInterfaceCount = false;

  constructor(
    private apiData: ApiDataService,
    private modificationService: ModificationService,
    private dialogService: DialogService,
    private deviceService: DeviceService,
    private router: Router) {
    super();
  }

  public initData(device: DeviceDto): void {
    // reset
    this.interfaceEditBlockSize = 1;
    this.deviceToEdit = device;
    this.deviceType = undefined;
    this.interfaceCount = 0;
    this.interfaceTemplates = [];
    this.availableInterfaceTemplates = [];
    this.interfaceNames = [];
    this.deviceName = '';
    this.comment = '';
    this.interfaceName = '';

    // init with device
    this.apiData
      .callWhenReady(() => this.deviceService.getDeviceDetails(device.deviceId))
      .pipe(takeUntil(this.destroy$))
      .subscribe(t => {
        this.deviceType = t.deviceType;
        this.interfaceCount = t.interfaces.length;
        for (let i = 0; i < this.interfaceCount; i++) {
          this.interfaceTemplates.push(new InterfaceTemplateSelection(t.interfaces[i].template,
            t.interfaces[i].streams.audioSend,
            t.interfaces[i].streams.audioReceive,
            t.interfaces[i].streams.videoSend,
            t.interfaces[i].streams.videoReceive,
            t.interfaces[i].streams.ancillarySend,
            t.interfaces[i].streams.ancillaryReceive));
          this.interfaceNames.push(t.interfaces[i].name);
          this.interfaceName = t.deviceType.interfaceNamePrefix;
        }
        this.availableInterfaceTemplates = t.deviceType.interfaceTemplates;
        this.allowModifyInterfaceCount = t.deviceType.flexibleStreams;
        this.deviceName = t.name;
        this.comment = t.comment;
      }, error => {
        throw error;
      });
  }

  public selectInterfaceTemplate(interfaceTemplate: string, index: number): void {
    for (let i = index;
         i < index + this.interfaceEditBlockSize && i < this.interfaceTemplates.length;
         i++) {
      const element = this.interfaceTemplates[i];
      element.templateName = interfaceTemplate;
    }
  }

  public isSelectableInterfaceTemplate(index: number): boolean {
    // only the indeces being multiples of the block size shall be selectable
    return index % this.interfaceEditBlockSize === 0;
  }

  onDeleteDevice() {
    const confirmBox = new ConfirmBoxInitializer();
    confirmBox.setTitle('Are you sure?');
    confirmBox.setMessage('Confirm to delete device: \'' + this.deviceName + '\'');
    // Set button labels, the first argument for the confirmation button, and the second one for the decline button.
    confirmBox.setButtonLabels('YES', 'NO');

    confirmBox.setConfig({
      disableIcon: true,
      allowHtmlMessage: false,
      buttonPosition: 'center',
      layoutType: DialogLayoutDisplay.DANGER
    });

    const subscription = confirmBox.openConfirmBox$().subscribe(resp => {
      subscription.unsubscribe();

      if (resp.success) {
        this.modificationService.deleteDevice(this.deviceToEdit, this.comment).catch(error => {
          throw error;
        });
        this.router.navigate(['/devices']);
      }
    });
  }

  onUpdateDevice() {
    this.modificationService.updateDevice(
      this.deviceToEdit.deviceId,
      this.interfaceTemplates,
      this.comment);
  }

  cancel() {
  }

  public interfacesNumber(size: number): void {

    this.interfaceCount = size;
    let currentInterfaceCount = this.interfaceTemplates.length;

    while (currentInterfaceCount !== size) {
      if (currentInterfaceCount < size) {
        this.interfaceTemplates.push(new InterfaceTemplateSelection(`${this.deviceType.interfaceTemplates[0].name}`,
          0, 0, 0, 0, 0, 0));
      } else {
        this.interfaceTemplates.pop();
      }

      currentInterfaceCount = this.interfaceTemplates.length;
    }
    this.addInterfaceName();
  }

  public addInterfaceName(): void {

    this.interfaceNames = Array(this.interfaceTemplates.length).fill(this.interfaceName)
      .map(
        (name: string, index) => name + String(index + 1)
          .padStart(2, '0')
      );
  }


  ngOnInit() {
    this.dialogService.editDevice = this;
  }
}
