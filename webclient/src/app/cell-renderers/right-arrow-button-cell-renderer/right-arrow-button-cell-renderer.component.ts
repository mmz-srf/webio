import {Component, OnInit} from '@angular/core';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import {ICellRendererParams} from 'ag-grid-community';

@Component({
    selector: 'app-right-arrow-button-cell-renderer',
    templateUrl: './right-arrow-button-cell-renderer.component.html',
    styleUrls: ['./right-arrow-button-cell-renderer.component.scss'],
    standalone: true
})
export class RightArrowButtonCellRendererComponent implements OnInit, ICellRendererAngularComp {
  constructor() {
  }

  ngOnInit() {
  }

  agInit(params: ICellRendererParams): void {
  }

  refresh(params: any): boolean {
    return false;
  }
}
