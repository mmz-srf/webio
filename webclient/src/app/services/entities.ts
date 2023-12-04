// enums

import {QueryResultState} from '../data/models/query-result-state';
import {FieldValueDto} from '../data/models/field-value-dto';

// dictionaries

export interface FieldValueDictionary {
  [key: string]: FieldValueDto;
}

// other

export interface ChangeEvent {
  comment: string;
}

export interface SelectedValue {
  value: string;
  backgroundColor: string;
  foregroundColor: string;
}

export interface EntityBase {
  id: string;
  deviceId: string;
  properties: FieldValueDictionary;
}

export interface PropertyChangedEvent {
  property: string;
  newValue: string;
  entity: string;
  device: string;
  entityType: string;
}

export interface QueryResult<T> {
  state?: QueryResultState;
  start?: number;
  count?: number;
  totalCount?: number;
  data?: T[];
}

export interface StreamTemplate {
  count: number;
  nameTemplate: string;
}
