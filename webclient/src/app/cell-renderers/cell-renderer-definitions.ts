import {FilterButtonCellRendererComponent} from './filter-button-cell-renderer/filter-button-cell-renderer.component';
import {EditButtonCellRendererComponent} from './edit-button-cell-renderer/edit-button-cell-renderer.component';
import {
  RightArrowButtonCellRendererComponent
} from './right-arrow-button-cell-renderer/right-arrow-button-cell-renderer.component';
import {IpAddressCellEditorComponent} from './ip-address-cell-editor/ip-address-cell-editor.component';
import {MaxLengthCellEditorComponent} from './max-length-cell-editor/max-length-cell-editor.component';
import {PlaceholderCellEditorComponent} from './placeholder-cell-editor/placeholder-cell-editor.component';
import {IsDuplicateCellEditorComponent} from './is-duplicate-cell-editor/is-duplicate-cell-editor.component';
import {SelectionCellEditorComponent} from "./selection-cell-editor/selection-cell-editor.component";

export const cellRendererDefinitions = {
  filterButton: FilterButtonCellRendererComponent,
  editButton: EditButtonCellRendererComponent,
  rightArrowButton: RightArrowButtonCellRendererComponent,

  ipAddressCellEditor: IpAddressCellEditorComponent,
  placeholderCellEditor: PlaceholderCellEditorComponent,
  maxLengthCellEditor: MaxLengthCellEditorComponent,
  isDuplicateCellEditor: IsDuplicateCellEditorComponent,
  selectionCellEditor: SelectionCellEditorComponent,
};

export const cellRendererNames = {
  filterButton: 'filterButton',
  editButton: 'editButton',
  rightArrowButton: 'rightArrowButton',

  ipAddressCellEditor: 'ipAddressCellEditor',
  placeholderCellEditor: 'placeholderCellEditor',
  maxLengthCellEditor: 'maxLengthCellEditor',
  isDuplicateCellEditor: 'isDuplicateCellEditor',
  selectionCellEditor: 'selectionCellEditor',
};
