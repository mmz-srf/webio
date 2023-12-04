import {Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {IAfterGuiAttachedParams, ICellEditorParams} from 'ag-grid-community';
import {AgEditorComponent} from 'ag-grid-angular';

@Component({
    selector: 'app-max-length-cell-editor',
    templateUrl: './max-length-cell-editor.component.html',
    styleUrls: ['./max-length-cell-editor.component.scss'],
    standalone: true
})
export class MaxLengthCellEditorComponent implements OnInit, AgEditorComponent {
  public maxLength: number;
  public placeholder: string;

  @ViewChild('inputElement', {static: true})
  private inputField: ElementRef<HTMLInputElement>;

  constructor() { }

  ngOnInit() {
  }

  agInit(params: ICellEditorParams): void {
    this.maxLength = (params as any).maxInputLength;
    this.placeholder = (params as any).placeholder;
  }

  afterGuiAttached(params?: IAfterGuiAttachedParams) {
    this.inputField.nativeElement.focus();
  }

  getValue(): any {
    return this.inputField.nativeElement.value;
  }
}
