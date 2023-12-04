import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {GenericDataSource} from '../common/generic-data-source';
import {ColumnHelperService} from '../services/column-helper.service';
import {DialogService} from '../services/dialog.service';
import {ModificationService} from '../services/modification.service';
import {SearchService} from '../services/search.service';
import {StreamService} from '../services/stream.service';
import {FilterService} from '../services/filter.service';
import {map, takeUntil} from 'rxjs/operators';
import {GridDataComponent} from '../common/grid-data-component';
import {cellRendererNames} from '../cell-renderers/cell-renderer-definitions';
import {ApiDataService} from '../services/api-data.service';
import {StreamDto} from '../data/models/stream-dto';
import {DataFieldDto} from '../data/models/data-field-dto';
import {EditButtonCellRendererComponent} from '../cell-renderers/edit-button-cell-renderer/edit-button-cell-renderer.component';
import {AgGridModule} from 'ag-grid-angular';

@Component({
  selector: 'app-streams',
  templateUrl: './streams.component.html',
  styleUrls: ['./streams.component.scss'],
  imports: [
    AgGridModule
  ],
  standalone: true
})
export class StreamsComponent extends GridDataComponent<StreamDto> implements OnInit {
  constructor(
    private streamService: StreamService,
    private modificationService: ModificationService,
    private dialogService: DialogService,
    private apiData: ApiDataService,
    private searchService: SearchService,
    private route: ActivatedRoute,
    private filters: FilterService,
    private columnHelper: ColumnHelperService) {
    super(
      modificationService,
      searchService,
      new GenericDataSource<StreamDto>(streamService));
    this.gridOptions.onGridReady = () => this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(_ => this.filters.apply(this.gridOptions.api));
  }

  applyFilter(): void {
    const filteredInterfaceName = this.route.snapshot.params.interfaceName;
    if (filteredInterfaceName) {
      let nameFilter = this.gridOptions.api.getFilterInstance('InterfaceName');
      if (!(nameFilter)) {
        nameFilter = this.gridOptions.api.getFilterInstance('InterfaceName_1');
      }
      if (!(nameFilter)) {
        setTimeout(() => this.applyFilter(), 500);
        return;
      }

      nameFilter.setModel({
        type: 'contains',
        filter: filteredInterfaceName
      });
      this.gridOptions.api.onFilterChanged();
    }
  }

  updateColumns(fields: DataFieldDto[], canEdit) {
    const fieldGroups = this.columnHelper.createColumns(fields, 'Stream', canEdit);

    if (canEdit) {

      // double click to edit
      const tagColumn = this.columnHelper.findColumn(fieldGroups, 'Tags');
      if (tagColumn) {
        tagColumn.editable = false;
      }

      // add navigation icon
      this.columnHelper.addColumnAfter(fieldGroups, 'Tags', {
        headerName: ' ',
        field: 'edit',
        width: 70,
        headerComponent: EditButtonCellRendererComponent,
        headerComponentParams: {
          onClick: this.editClicked.bind(this),
          dataTarget: '#editStreamModal'
        },
        onCellClicked: () => this.editClicked(),
        cellRenderer: cellRendererNames.editButton,
        cellRendererParams: {dataTarget: '#editStreamModal'},
        suppressSizeToFit: true,
        filter: false,
      });
    }

    this.columnHelper.findColumn(fieldGroups, 'DeviceName').sort = 'asc';

    this.gridOptions.columnDefs = fieldGroups;
    if (this.gridOptions.api) {
      this.gridOptions.api.setColumnDefs(fieldGroups);
      this.gridOptions.api.sizeColumnsToFit();
      this.gridOptions.columnApi.applyColumnState({
        state: [
          {
            colId: 'DeviceName',
            sort: 'asc',
            sortIndex: 0
          },
          {
            colId: 'InterfaceName',
            sort: 'asc',
            sortIndex: 1
          },
          {
            colId: 'Name',
            sort: 'asc',
            sortIndex: 2,
          }
        ]
      });
    }

    this.dialogService.gridOptions = this.gridOptions;

    this.filters.apply(this.gridOptions.api);
    this.addClickListenerToEditHeader();
  }

  editClicked() {
    const streams = this.gridOptions.api.getSelectedNodes().map(st => st.data as StreamDto);
    if (streams) {
      this.dialogService.editStream.initData(streams);
    }
  }

  ngOnInit() {
    super.onInit();
    this.apiData.callWhenReady((canEdit) => this.streamService.getFields().pipe(map(fields => ({canEdit, fields}))))
      .pipe(takeUntil(this.destroy$))
      .subscribe(({canEdit, fields}) => this.updateColumns(fields, canEdit));

    this.dialogService.editStream.dataChanged$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.gridOptions.api.setDatasource(this.gridDataSource));
  }

  // todo: fix this with pb
  private addClickListenerToEditHeader() {
    // unfortunately, ag-grid does not support setting a click event on the header or the template, which means we have to do this
    // programmatically
    const headers = Array.from(document.getElementsByClassName('ag-header-cell') as HTMLCollectionOf<HTMLElement>);
    const editButtonHeader = headers.filter(e => e.getAttribute('col-id') === 'edit')[0];

    if (editButtonHeader) {
      editButtonHeader.addEventListener('click', () => this.editClicked());
    }
  }
}
