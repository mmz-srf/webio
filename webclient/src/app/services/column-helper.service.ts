import {Injectable} from '@angular/core';
import {CellClassParams, ColDef, ColGroupDef, ValueGetterParams, ValueSetterParams} from 'ag-grid-community';
import {EntityBase, SelectedValue} from './entities';
import {ModificationService} from './modification.service';
import {cellRendererNames} from '../cell-renderers/cell-renderer-definitions';
import {DataFieldDto} from '../data/models/data-field-dto';
import {ColumnVisibilityDto, DataFieldTypeDto} from '../data/models';
import {SelectionOptionsService} from './selection-options.service';
import {groupBy} from '../common/utils';

@Injectable()
export class ColumnHelperService {
  constructor(private modificationService: ModificationService,
              private selectionOptionsService: SelectionOptionsService) {
  }

  public createColumns(fields: DataFieldDto[], entityType: string, canEdit: boolean): ColGroupDef[] {
    return Object.entries(groupBy(fields, f => f.category))
      .map(([category, fieldGroup]) => {
        return {
          headerName: category,
          children: this.mapColumnsOfGroup(fieldGroup, canEdit, entityType),
        };
      });
  }

  private mapColumnsOfGroup(fieldGroup: DataFieldDto[], canEdit: boolean, entityType: string) {
    return fieldGroup.map((field) => {
      const result = {
        field: field.key,
        headerName: field.displayName,
        headerTooltip: field.description,
        editable: (field.readonly !== true) && canEdit,
        cellEditorParams: {},

        valueGetter: x => this.onGetValue(x),
        valueSetter: x => this.onSetValue(x, entityType),
        columnGroupShow: this.translateColumnGroupShow(field),
        cellClassRules: {
          bold: this.isDirty,
          italic: this.isInherited
        },
        suppressMenu: true,
        suppressSizeToFit: true,
      } as ColDef;

      this.setCellEditor(field, result);
      return result;
    });
  }

  private setCellEditor(field: DataFieldDto, result: ColDef) {
    switch (field.fieldType) {
      case DataFieldTypeDto.Selection:
        this.useSelectionCellEditor(result, field);
        break;
      case DataFieldTypeDto.Boolean:
        this.useBooleanCellEditor(result);
        break;
      case DataFieldTypeDto.IpAddress:
        this.useIpAddressCellEditor(result);
        break;
      case DataFieldTypeDto.Text:
      default:
        if (field.key === 'Name') {
          this.useDuplicationCheckingCellEditor(result, field);
        } else if (field.maxLength > 0) {
          this.useLimitedLengthCellEditor(result, field);
        } else {
          this.usePlaceholderCellEditor(result, field);
        }
        break;
    }
  }

  private usePlaceholderCellEditor(result: ColDef, field: DataFieldDto) {
    result.cellEditor = cellRendererNames.placeholderCellEditor;
    result.cellEditorParams = {
      placeholder: field.placeholder ? field.placeholder : '',
    };
  }

  private useLimitedLengthCellEditor(result: ColDef, field: DataFieldDto) {
    result.cellEditor = cellRendererNames.maxLengthCellEditor;
    result.cellEditorParams = {
      maxInputLength: field.maxLength,
      placeholder: field.placeholder ? field.placeholder : '',
    };
  }

  private useDuplicationCheckingCellEditor(result: ColDef, field: DataFieldDto) {
    result.cellEditor = cellRendererNames.isDuplicateCellEditor;
    result.cellEditorParams = {
      placeholder: field.placeholder ? field.placeholder : '',
    };
  }

  private useIpAddressCellEditor(result: ColDef) {
    result.cellEditor = cellRendererNames.ipAddressCellEditor;
  }

  private useBooleanCellEditor(result: ColDef) {
    result.cellEditor = 'agSelectCellEditor';
    result.cellEditorParams = {values: ['', 'true', 'false']};
  }

  private useSelectionCellEditor(result: ColDef, field: DataFieldDto) {
    result.cellEditorPopup = true;
    result.cellEditor = cellRendererNames.selectionCellEditor;
    result.cellRendererParams = {selectedValues: field.selectableValuesExt};
    result.cellEditorParams = (params) => {
      const refValue = params.data.properties[field.selectableValuesFactory.referenceField]?.value;
      return {
        values: refValue ? this.selectionOptionsService.getSelectableValues(field, refValue) : [''].concat(field.selectableValues),
      };
    };
  }


  private getBackgroundColor(params: SelectedValue[], value: string): string {
    const index = params.findIndex(p => p.value === value);
    if (index < 0) {
      return null;
    }

    return params[index].backgroundColor;

  }

  private getForegroundColor(params: SelectedValue[], value: string): string {
    const index = params.findIndex(p => p.value === value);
    if (index < 0) {
      return null;
    }

    return params[index].foregroundColor;

  }

  private translateColumnGroupShow(field: DataFieldDto): string {
    if (field.visible === ColumnVisibilityDto.Always) {
      return 'always';
    }
    if (field.visible === ColumnVisibilityDto.Collapsed) {
      return 'closed';
    }
    return 'open';
  }

  addColumnAfter(fieldGroups: ColGroupDef[], referenceColumnName: string, column: ColDef): void {
    const groupIndex = fieldGroups.findIndex(g => g.children.find(col => (col as ColDef).field === referenceColumnName));
    if (groupIndex < 0) {
      console.error(`column ${referenceColumnName} not found`);
      return;
    }
    const colIndex = fieldGroups[groupIndex].children.findIndex(col => (col as ColDef).field === referenceColumnName);
    // console.log(`column ${referenceColumnName} found (${groupIndex}, ${colIndex})`);

    fieldGroups[groupIndex].children.splice(colIndex + 1, 0, column);
  }

  findColumn(fieldGroups: ColGroupDef[], columnName: string): ColDef {
    const groupIndex = fieldGroups.findIndex(g => g.children.find(col => (col as ColDef).field === columnName));
    if (groupIndex < 0) {
      console.error(`column ${columnName} not found`);
      return undefined;
    }
    const colIndex = fieldGroups[groupIndex].children.findIndex(col => (col as ColDef).field === columnName);
    // console.log(`column ${columnName} found (${groupIndex}, ${colIndex})`);

    // double click to edit
    return fieldGroups[groupIndex].children[colIndex] as ColDef;
  }

  isDirty(params: CellClassParams): boolean {
    const entity = params.data as EntityBase;
    if (entity) {
      return entity.properties[params.colDef.field].dirty === true;
    }
  }

  isInherited(params: CellClassParams): boolean {
    const iface = params.data as EntityBase;
    if (iface) {
      return iface.properties[params.colDef.field].inherited === true;
    }
    return false;
  }

  onGetValue(params: ValueGetterParams): string {
    const entity = params.data as EntityBase;
    if (entity) {
      return entity.properties[params.colDef.field].value;
    }
    return '???';
  }

  onSetValue(params: ValueSetterParams, entityType: string): boolean {
    if (params.oldValue !== params.newValue) {
      const entity = params.data as EntityBase;
      const propertyValue = entity.properties[params.colDef.field];

      // update model
      propertyValue.value = params.newValue;
      propertyValue.dirty = true;

      // remember modification
      this.modificationService.changeValue(
        params.colDef.field,
        params.newValue,
        entity.id,
        entity.deviceId,
        entityType);

      return true;
    } else {
      return false;
    }
  }
}

