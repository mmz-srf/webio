import {AfterViewInit, Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {Router} from '@angular/router';
import {ModificationService} from '../services/modification.service';
import {DestructibleComponent} from '../common/destructible-component';
import {debounce, takeUntil} from 'rxjs/operators';
import {DeviceService} from '../services/device.service';
import {ApiDataService} from '../services/api-data.service';
import {DeviceTypeDto} from '../data/models/device-type-dto';
import {InterfaceTemplateDto} from '../data/models/interface-template-dto';
import {InterfaceTemplateSelectionDto} from '../data/models/interface-template-selection-dto';
import {fromEvent, interval, of} from 'rxjs';
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
  selector: 'app-add-device',
  templateUrl: './add-device.component.html',
  styleUrls: ['./add-device.component.scss'],
  imports: [
    FormsModule,
    NgForOf,
    NgIf
  ],
  standalone: true
})
export class AddDeviceComponent extends DestructibleComponent implements OnInit, AfterViewInit {

  constructor(
    private apiDataService: ApiDataService,
    private deviceService: DeviceService,
    private modificationService: ModificationService,
    private router: Router) {
    super();
  }

  public deviceName = '';
  public comment = 'creating new device';

  public deviceTypes: DeviceTypeDto[];
  public selectedDeviceType: DeviceTypeDto;
  public availableInterfaceTemplates: InterfaceTemplateDto[] = [];

  public isDuplicateDeviceName = false;
  @ViewChild('deviceInput', {static: true}) deviceInputRef: ElementRef;

  public interfaceCount = 0;
  public interfaceList = [];
  public interfaceTemplates: InterfaceTemplateSelectionDto[] = [];
  public interfaceEditBlockSize = 1;
  public interfaceNames: string[] = [];
  public interfaceName: string;
  public vidsend;
  vidrec;
  audsend;
  audrec;
  ancsend;
  ancrec;
  public useSt20227: boolean;


  public allowModifyInterfaceCount = false;

  public selectInterfaceTemplate(interfaceTemplate: string, index: number): void {
    for (let i = index;
         i < index + this.interfaceEditBlockSize && i < this.interfaceTemplates.length;
         i++) {
      const element = this.interfaceTemplates[i];
      element.templateName = interfaceTemplate;
    }
    this.interfaceTemplates[index].templateName = interfaceTemplate;
  }

  public isSelectableInterfaceTemplate(index: number): boolean {
    // only the indeces being multiples of the block size shall be selectable
    return index % this.interfaceEditBlockSize === 0;
  }

  public interfacesNumber(size: number): void {

    this.interfaceCount = size;
    let currentInterfaceCount = this.interfaceTemplates.length;

    while (currentInterfaceCount !== size) {
      if (currentInterfaceCount < size) {
        this.interfaceTemplates.push(new InterfaceTemplateSelection(`${this.selectedDeviceType.interfaceTemplates[0].name}`,
          this.audsend, this.audrec, this.vidsend, this.vidrec, this.ancsend, this.ancrec));
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


  public selectDeviceType(deviceTypeName: string): void {
    this.empty();
    this.selectedDeviceType = this.deviceTypes.find(d => d.name === deviceTypeName);
    if (this.selectedDeviceType) {
      this.availableInterfaceTemplates = this.selectedDeviceType.interfaceTemplates;
      this.interfaceCount = this.selectedDeviceType.interfaceCount;
      for (let i = 0; i < this.interfaceCount; i++) {
        const nr = String(i + 1).padStart(2, '0');
        this.interfaceNames.push(`${this.selectedDeviceType.interfaceNamePrefix}${nr}`);
      }
      this.setStreams();

      this.interfaceTemplates = this.selectedDeviceType.defaultInterfaces.map(
        (x) => new InterfaceTemplateSelection(`${x}`, this.audsend, this.audrec,
          this.vidsend, this.vidrec, this.ancsend, this.ancrec));
      this.interfaceName = this.selectedDeviceType.interfaceNamePrefix;
      if (!this.selectedDeviceType.flexibleStreams) {
        this.allowModifyInterfaceCount = false;
      } else {
        this.allowModifyInterfaceCount = true;
        this.interfaceList.push(this.interfaceCount);
      }
    } else {
      this.empty();
    }
  }

  private setStreams() {
    this.vidsend = 0;
    this.vidrec = 0;
    this.audsend = 0;
    this.audrec = 0;
    this.ancsend = 0;
    this.ancrec = 0;
    this.availableInterfaceTemplates.forEach(streams => {
      streams.streams.forEach(stream => {
        switch (stream.nameTemplate) {
          case 'VIDsend': {
            this.vidsend = stream.count;
            break;
          }
          case 'VIDrec': {
            this.vidrec = stream.count;
            break;
          }
          case 'AUDsend': {
            this.audsend = stream.count;
            break;
          }
          case 'AUDrec': {
            this.audrec = stream.count;
            break;
          }
          case 'ANCsend': {
            this.ancsend = stream.count;
            break;
          }
          case 'ANCrec': {
            this.ancrec = stream.count;
            break;
          }
        }
      });
    });

  }


  private empty() {
    this.interfaceCount = 0;
    this.interfaceNames = [];
    this.availableInterfaceTemplates = [];
    this.interfaceTemplates = [];
  }

  public onCreateDevice() {
    of(this.modificationService.createDevice(
      this.deviceName,
      this.selectedDeviceType.name,
      this.interfaceTemplates,
      this.comment,
      this.useSt20227))
      .pipe(
        takeUntil(this.destroy$))
      .subscribe(() => this.router.navigate(['/devices/detail'], {queryParams: {deviceName: this.deviceName}}));
  }

  cancel() {
  }

  get hasBlockSize(): boolean {
    return this.selectedDeviceType.defaultInterfaces.length > 1;
  }

  public ngOnInit() {
    this.apiDataService
      .callWhenReady(() => this.deviceService.getDeviceTypes())
      .pipe(takeUntil(this.destroy$))
      .subscribe(x => {
        this.deviceTypes = x;
      }, err => {
        throw err;
      });
  }

  ngAfterViewInit() {
    const $input = fromEvent(this.deviceInputRef.nativeElement, 'input');
    const result = $input.pipe(debounce(() => interval(250)));
    result.subscribe(() => this.onDeviceNameChange());
  }

  onDeviceNameChange() {
    const deviceName = this.deviceName.toLowerCase();
    this.apiDataService
      .callWhenReady(() => this.deviceService.isDuplicateDeviceName(deviceName))
      .pipe(takeUntil(this.destroy$))
      .subscribe(result => {
        this.isDuplicateDeviceName = result;
      }, err => {
        throw err;
      });
  }
}
