import {Component, OnInit} from '@angular/core';
import {ICellRendererAngularComp, IHeaderAngularComp} from 'ag-grid-angular';
import {ICellRendererParams, IHeaderParams} from 'ag-grid-community';

@Component({
    selector: 'app-edit-button-cell-renderer',
    templateUrl: './edit-button-cell-renderer.component.html',
    styleUrls: ['./edit-button-cell-renderer.component.scss'],
    standalone: true
})
export class EditButtonCellRendererComponent implements OnInit, ICellRendererAngularComp, IHeaderAngularComp {
  dataTarget: string;
  clickHandler = () => {};

  constructor() {
  }

  ngOnInit() {
  }

  agInit(params: ICellRendererParams | IHeaderParams): void {
    const parameters = params as any;
    if (parameters.colDef) {
      this.dataTarget = parameters.colDef?.cellRendererParams?.dataTarget;
    } else {
      if (parameters.onClick) {
        this.clickHandler = parameters.onClick;
        this.dataTarget = parameters.dataTarget;
      }
    }
  }

  refresh(params: any): boolean {
    return false;
  }
}
