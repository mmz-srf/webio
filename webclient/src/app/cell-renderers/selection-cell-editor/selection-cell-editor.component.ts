import {AfterViewInit, Component, ViewChild} from '@angular/core';
import {ICellEditorAngularComp} from "ag-grid-angular";
import {NgSelectComponent, NgSelectModule} from '@ng-select/ng-select';
import {NgForOf} from '@angular/common';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-selection-cell-editor',
  templateUrl: './selection-cell-editor.component.html',
  styleUrls: ['./selection-cell-editor.component.scss'],
  imports: [
    NgSelectModule,
    NgForOf,
    FormsModule
  ],
  standalone: true
})
export class SelectionCellEditorComponent implements ICellEditorAngularComp, AfterViewInit {
  params: any;
  selectedValue: number;

  @ViewChild('ngSelect') ngSelect: NgSelectComponent;

  ngAfterViewInit() {
    window.setTimeout(() => {
      this.ngSelect.focus();
    });
  }

  agInit(params: any): void {
    this.params = params;
  }

  getValue(): any {
    return this.selectedValue;
  }

  isPopup(): boolean {
    return true;
  }

  onChange() {
    this.params.stopEditing();
  }
}
