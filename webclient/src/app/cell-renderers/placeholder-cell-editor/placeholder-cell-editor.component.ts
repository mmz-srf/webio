import {Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {AgEditorComponent} from 'ag-grid-angular';
import {IAfterGuiAttachedParams, ICellEditorParams} from 'ag-grid-community';

@Component({
    selector: 'app-placeholder-cell-editor',
    templateUrl: './placeholder-cell-editor.component.html',
    styleUrls: ['./placeholder-cell-editor.component.scss'],
    standalone: true
})
export class PlaceholderCellEditorComponent implements OnInit, AgEditorComponent {
  public placeholder: string;

  @ViewChild('inputElement', {static: true})
  private inputField: ElementRef<HTMLInputElement>;

  constructor() { }

  ngOnInit() {
  }

  agInit(params: ICellEditorParams): void {
    this.placeholder = (params as any).placeholder;
    this.inputField.nativeElement.value = params.value;
  }

  afterGuiAttached(params?: IAfterGuiAttachedParams) {
    this.inputField.nativeElement.focus();
  }

  getValue(): any {
    return this.inputField.nativeElement.value;
  }
}
