import {IEntityService} from '../services/device.service';
import {IDatasource, IGetRowsParams} from 'ag-grid-community';

export class GenericDataSource<T> implements IDatasource {
  constructor(private service: IEntityService<T>) {
  }

  rowCount?: number;

  getRows(params: IGetRowsParams): void {
    const count = params.endRow - params.startRow;
    let sort = '';
    let sortOrder = '';
    if (params.sortModel && params.sortModel.length > 0) {
      params.sortModel.forEach(sortModel => {
        let s = sortModel.colId;
        const so = sortModel.sort;

        if (s.endsWith('_1')) {
          s = s.substr(0, s.length - 2);
        }

        sort = `${sort},${s}`;
        sortOrder = `${sortOrder},${so}`;
      });

      sort = sort.replace(/^,+|,+$/g, '');
      sortOrder = sortOrder.replace(/^,+|,+$/g, '');
    }

    const filters = new Map<string, string>();

    if (params.filterModel) {
      Object.keys(params.filterModel).forEach(key => {

        const filterValue = params.filterModel[key];

        if (key.endsWith('_1')) {
          key = key.substr(0, key.length - 2);
        }

        filters.set(key, filterValue.filter);
      });
    }

    this.service.getEntities(params.startRow, count, sort, sortOrder, filters)
      .subscribe(result => {
        params.successCallback(result.data, result.totalCount);
      }, err => {
        throw err;
      });
  }

  destroy?(): void {
  }
}
