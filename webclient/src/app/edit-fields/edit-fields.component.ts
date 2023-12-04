import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {EntityBase} from '../services/entities';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {ApiDataService} from '../services/api-data.service';
import {DataFieldDto} from '../data/models/data-field-dto';
import {DataFieldTypeDto} from '../data/models';
import {SelectionOptionsService} from '../services/selection-options.service';
import {AsyncPipe, NgForOf, NgSwitch, NgSwitchCase, NgSwitchDefault} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {NgSelectModule} from '@ng-select/ng-select';

interface FieldProvider {
  getFields(): Observable<DataFieldDto[]>;
}

@Component({
  selector: 'app-edit-fields',
  templateUrl: './edit-fields.component.html',
  styleUrls: ['./edit-fields.component.scss'],
  imports: [
    AsyncPipe,
    NgSwitchDefault,
    FormsModule,
    NgSelectModule,
    NgSwitchCase,
    NgSwitch,
    NgForOf
  ],
  standalone: true
})
export class EditFieldsComponent implements OnInit {
  private excludedFields = ['Tags'];

  @Input()
  public entities$: Observable<EntityBase[]>;

  @Input()
  public fieldProvider: FieldProvider;

  @Output()
  public fieldChanged = new EventEmitter<{ field: DataFieldDto, newVal: string }>();

  editableStreamFields$: Observable<DataFieldDto[]>;

  fieldType = DataFieldTypeDto;

  constructor(private apiData: ApiDataService, public selectOptionServices: SelectionOptionsService) {
  }

  ngOnInit() {
    this.editableStreamFields$ = this.getEditableFields$();
  }

  private getEditableFields$() {
    return this.apiData.callWhenReady(() => this.fieldProvider.getFields())
      .pipe(
        map(fields => fields
          .filter(f => !f.readonly)
          .filter(f => !this.excludedFields.includes(f.key))));
  }

  getValueIfAllEqual$(field: DataFieldDto): Observable<string | boolean> {
    return this.entities$.pipe(
      map(streams => this.getConsistentValueOrEmpty(streams, field)),
    );
  }

  private getConsistentValueOrEmpty(streams: EntityBase[], field: DataFieldDto): string | boolean {
    const values = this.getAllValues(streams, field);

    if (!values || values.length === 0) {
      return this.getDefaultForType(field.fieldType);
    }

    return values.every(v => v === values[0])
      ? this.coerceType(values[0], field.fieldType)
      : this.getDefaultForType(field.fieldType);
  }

  private getAllValues(streams: EntityBase[], field: DataFieldDto) {
    return streams.map(s => s.properties[field.key].value);
  }

  private getDefaultForType(fieldType: DataFieldTypeDto): string | boolean {
    switch (fieldType) {
      case DataFieldTypeDto.Boolean:
        return false;
      default:
        return '';
    }
  }

  private coerceType(value: string, fieldType: DataFieldTypeDto) {
    switch (fieldType) {
      case DataFieldTypeDto.Boolean:
        return 'true' === value;
      default:
        return value;
    }
  }

  change(field: DataFieldDto, $event: Event) {
    const input = $event.target as HTMLInputElement;
    this.fieldChanged.emit({field, newVal: `${input?.value}`});
  }

  changeSelection(field: DataFieldDto, value) {
    this.fieldChanged.emit({field, newVal: value});
  }

  changeCheckbox(field: DataFieldDto, $event: Event) {
    const input = $event.target as HTMLInputElement;
    this.fieldChanged.emit({field, newVal: `${input.checked}`});
  }

  getOptions(field: DataFieldDto) {
    if (field.selectableValues?.length) {
      return field.selectableValues;
    }
    return this.selectOptionServices.getSelectableValues(field, '');
  }
}
