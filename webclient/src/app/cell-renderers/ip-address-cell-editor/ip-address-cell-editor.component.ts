import {Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {AgEditorComponent} from 'ag-grid-angular';
import {IAfterGuiAttachedParams, ICellEditorParams} from 'ag-grid-community';

@Component({
    selector: 'app-ip-address-cell-editor',
    templateUrl: './ip-address-cell-editor.component.html',
    styleUrls: ['./ip-address-cell-editor.component.scss'],
    standalone: true
})
export class IpAddressCellEditorComponent implements OnInit, AgEditorComponent {
  constructor() {
  }

  private static ipv4ipv6regex = /((^\s*(((\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.){3}(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5]))\s*$)|(^\s*((([\dA-Fa-f]{1,4}:){7}([\dA-Fa-f]{1,4}|:))|(([\dA-Fa-f]{1,4}:){6}(:[\dA-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([\dA-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[\dA-Fa-f]{1,4}){1,7})|((:[\dA-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$))/;

  @ViewChild('ipAddress', {static: true})
  private ipAddress: ElementRef<HTMLInputElement>;

  ngOnInit() {
  }

  agInit(params: ICellEditorParams): void {
    this.ipAddress.nativeElement.value = params.value;
  }

  afterGuiAttached(params?: IAfterGuiAttachedParams) {
    this.ipAddress.nativeElement.focus();
  }

  getValue(): any {
    if (!this.isValid()) {
      throw new Error('IP address has to be a valid IPv4 or IPv6 address');
    }
    return this.ipAddress.nativeElement.value;
  }

  isValid(): boolean {
    const val = this.ipAddress.nativeElement.value;
    if (!val || val.length === 0) {
      return true;
    }
    return IpAddressCellEditorComponent.ipv4ipv6regex.test(val);
  }
}
