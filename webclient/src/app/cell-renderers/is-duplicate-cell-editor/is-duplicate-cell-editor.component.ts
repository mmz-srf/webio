import {AfterViewInit, Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {AgEditorComponent} from 'ag-grid-angular';
import {IAfterGuiAttachedParams, ICellEditorParams} from 'ag-grid-community';
import {debounce, takeUntil} from 'rxjs/operators';
import {ApiDataService} from '../../services/api-data.service';
import {DeviceService} from '../../services/device.service';
import {DestructibleComponent} from '../../common/destructible-component';
import {fromEvent, interval} from 'rxjs';

@Component({
    selector: 'app-is-duplicate-cell-editor',
    templateUrl: './is-duplicate-cell-editor.component.html',
    styleUrls: ['./is-duplicate-cell-editor.component.scss'],
    standalone: true
})
export class IsDuplicateCellEditorComponent extends DestructibleComponent implements OnInit, AfterViewInit, AgEditorComponent {
  public placeholder: string;
  public isDuplicateDeviceName = false;

  private deviceId = '';

  @ViewChild('inputElement', {static: true})
  private inputField: ElementRef<HTMLInputElement>;

  constructor(
    private apiDataService: ApiDataService,
    private deviceService: DeviceService,
  ) {
    super();
  }

  ngOnInit() {
  }

  agInit(params: ICellEditorParams): void {
    this.placeholder = (params as any).placeholder;
    this.inputField.nativeElement.value = params.value;
    this.deviceId = params.data.deviceId;
  }

  afterGuiAttached(params?: IAfterGuiAttachedParams) {
    this.inputField.nativeElement.focus();
  }

  getValue(): any {
    if (this.isDuplicateDeviceName) {
      throw new Error('Device name already exists');
    }
    return this.inputField.nativeElement.value;
  }

  ngAfterViewInit() {
    const $input = fromEvent(this.inputField.nativeElement, 'input');
    const result = $input.pipe(debounce(() => interval(200)));
    result.subscribe(() => this.onDeviceNameChange());
  }

  onDeviceNameChange(): void {
    const value = this.inputField.nativeElement.value.toLowerCase();
    this.apiDataService
      .callWhenReady(() => this.deviceService.isDuplicateDeviceName(value, this.deviceId))
      .pipe(takeUntil(this.destroy$)).subscribe(result => {
        this.isDuplicateDeviceName = result;
    });
  }
}
