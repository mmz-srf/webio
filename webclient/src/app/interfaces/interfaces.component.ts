import {Component, OnInit} from '@angular/core';
import {InterfaceService} from '../services/interface.service';
import {CellClickedEvent, ColDef, ColumnState} from 'ag-grid-community';
import {ModificationService} from '../services/modification.service';
import {ActivatedRoute, Router} from '@angular/router';
import {GenericDataSource} from '../common/generic-data-source';
import {SearchService} from '../services/search.service';
import {DialogService} from '../services/dialog.service';
import {ColumnHelperService} from '../services/column-helper.service';
import {FilterService} from '../services/filter.service';
import {GridDataComponent} from '../common/grid-data-component';
import {cellRendererNames} from '../cell-renderers/cell-renderer-definitions';
import {ApiDataService} from '../services/api-data.service';
import {map, takeUntil} from 'rxjs/operators';
import {DataFieldDto, InterfaceDto} from '../data/models';
import {EditButtonCellRendererComponent} from '../cell-renderers/edit-button-cell-renderer/edit-button-cell-renderer.component';
import {AgGridModule} from 'ag-grid-angular';

@Component({
  selector: 'app-interfaces',
  templateUrl: './interfaces.component.html',
  styleUrls: ['./interfaces.component.scss'],
  imports: [
    AgGridModule
  ],
  standalone: true
})
export class InterfacesComponent extends GridDataComponent<InterfaceDto> implements OnInit {

  constructor(
    private interfaceService: InterfaceService,
    private modificationService: ModificationService,
    private dialogService: DialogService,
    private router: Router,
    private apiData: ApiDataService,
    private searchService: SearchService,
    private route: ActivatedRoute,
    private filters: FilterService,
    private columnHelper: ColumnHelperService) {
    super(
      modificationService,
      searchService,
      new GenericDataSource<InterfaceDto>(interfaceService));

    this.gridOptions.onGridReady = () => this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(_ => this.filters.apply(this.gridOptions.api));
  }

  updateColumns(fields: DataFieldDto[], canEdit: boolean) {
    const fieldGroups = this.columnHelper.createColumns(fields, 'Interface', canEdit); // no editing in the interface view

    // double click on second column navigates to streams
    const secondColumn = fieldGroups[0].children[1] as ColDef;
    secondColumn.onCellDoubleClicked = (x) => this.detailClicked(x);

    // add navigation icon
    this.columnHelper.addColumnAfter(fieldGroups, 'Name', {
      headerName: ' ',
      field: 'detail',
      width: 50,
      onCellClicked: x => this.detailClicked(x),
      cellRenderer: cellRendererNames.rightArrowButton,
      suppressSizeToFit: true,
      filter: false,
    });

    if (canEdit) {
      // add edit icon
      this.columnHelper.addColumnAfter(fieldGroups, 'detail', {
        headerName: ' ',
        field: 'edit',
        width: 70,
        headerComponent: EditButtonCellRendererComponent,
        headerComponentParams: {
          onClick: this.editClicked.bind(this),
          dataTarget: '#editInterfaceModal'
        },
        onCellClicked: () => this.editClicked(),
        cellRenderer: cellRendererNames.editButton,
        cellRendererParams: {dataTarget: '#editInterfaceModal'},
        suppressSizeToFit: true,
        filter: false,
      });
    }

    this.columnHelper.findColumn(fieldGroups, 'DeviceName').sort = 'asc';

    this.gridOptions.columnDefs = fieldGroups;

    const defaultSortModel: ColumnState[] = [
      {
        colId: 'DeviceName',
        sort: 'asc',
        sortIndex: 0,
      },
      {
        colId: 'Name',
        sort: 'asc',
        sortIndex: 1
      }
    ];

    if (this.gridOptions.api) {
      this.gridOptions.api.setColumnDefs(fieldGroups);
      this.gridOptions.api.sizeColumnsToFit();
      this.gridOptions.columnApi.applyColumnState({state: defaultSortModel});
    }
    this.dialogService.gridOptions = this.gridOptions;

    this.filters.apply(this.gridOptions.api);
    this.addClickListenerToEditHeader();
  }

  async detailClicked(event: CellClickedEvent) {
    const iface = event.data as InterfaceDto;
    if (iface) {
      await this.router.navigate(['/streams'], {
        queryParams: {interfaceName: iface.properties.Name.value},
        queryParamsHandling: 'merge',
      });
    }
  }

  ngOnInit() {
    super.onInit();
    this.apiData.callWhenReady((canEdit) => this.interfaceService.getFields().pipe(map(fields => ({fields, canEdit}))))
      .pipe(takeUntil(this.destroy$))
      .subscribe(({fields, canEdit}) => this.updateColumns(fields, canEdit));

    this.dialogService.editInterface.dataChanged$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.gridOptions.api.setDatasource(this.gridDataSource));
  }

  private editClicked() {
    const interfaces = this.gridOptions.api.getSelectedNodes().map(iface => iface.data as InterfaceDto);
    if (interfaces) {
      this.dialogService.editInterface.initData(interfaces);
    }
  }

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
