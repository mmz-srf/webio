import {DestructibleComponent} from './destructible-component';
import {takeUntil} from 'rxjs/operators';
import {ModificationService} from '../services/modification.service';
import {SearchService} from '../services/search.service';
import {GridOptions} from 'ag-grid-community';
import {GenericDataSource} from './generic-data-source';
import {cellRendererDefinitions} from '../cell-renderers/cell-renderer-definitions';

export abstract class GridDataComponent<T> extends DestructibleComponent {
  gridOptions: GridOptions;

  protected constructor(
    private modifications: ModificationService,
    private search: SearchService,
    protected gridDataSource: GenericDataSource<T>,
  ) {
    super();
    this.gridOptions = {
      singleClickEdit: true,
      rowModelType: 'infinite',
      rowSelection: 'multiple',
      multiSortKey: 'ctrl',
      components: cellRendererDefinitions,
      defaultColDef: {
        floatingFilter: true,
        sortable: true,
        filter: true,
        resizable: true,
      },
      datasource: gridDataSource,
    };
  }

  public onInit() {
    this.search.globalSearchQueryChanged$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.gridOptions.api.setDatasource(this.gridDataSource);
      });

    this.modifications.dataChanged$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.gridOptions.api.setDatasource(this.gridDataSource);
      });
  }
}
