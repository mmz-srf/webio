import {AfterViewInit, Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {CellClickedEvent, ColDef, GridOptions} from 'ag-grid-community';
import {GenericDataSource} from '../common/generic-data-source';
import {ColumnHelperService} from '../services/column-helper.service';
import {DeviceService} from '../services/device.service';
import {DialogService} from '../services/dialog.service';
import {InterfaceService} from '../services/interface.service';
import {ModificationService} from '../services/modification.service';
import {SearchService} from '../services/search.service';
import {StreamService} from '../services/stream.service';
import {FilterService} from '../services/filter.service';
import {DestructibleComponent} from '../common/destructible-component';
import {map, takeUntil} from 'rxjs/operators';
import {cellRendererDefinitions, cellRendererNames} from '../cell-renderers/cell-renderer-definitions';
import {AuthorizationService} from '../services/authorization.service';
import {MsalBroadcastService} from '@azure/msal-angular';
import {ApiDataService} from '../services/api-data.service';
import {DeviceDto} from '../data/models/device-dto';
import {DataFieldDto} from '../data/models/data-field-dto';
import {StreamsComponent} from '../streams/streams.component';
import {InterfacesComponent} from '../interfaces/interfaces.component';
import {AgGridModule} from 'ag-grid-angular';

@Component({
  selector: 'app-device-detail',
  templateUrl: './device-detail.component.html',
  styleUrls: ['./device-detail.component.scss'],
  imports: [
    StreamsComponent,
    InterfacesComponent,
    AgGridModule
  ],
  standalone: true
})
export class DeviceDetailComponent extends DestructibleComponent implements OnInit, AfterViewInit {

  constructor(
    private broadcastService: MsalBroadcastService,
    private authorizationService: AuthorizationService,
    private apiData: ApiDataService,
    private deviceService: DeviceService,
    private interfaceService: InterfaceService,
    private streamService: StreamService,
    private modificationService: ModificationService,
    private dialogService: DialogService,
    private router: Router,
    private searchService: SearchService,
    private route: ActivatedRoute,
    private filters: FilterService,
    private columnHelper: ColumnHelperService) {
    super();
  }

  gridDataSourceDevice = new GenericDataSource<DeviceDto>(this.deviceService);
  gridOptionsDevice: GridOptions = {
    singleClickEdit: true,
    rowModelType: 'infinite',
    datasource: this.gridDataSourceDevice,
    multiSortKey: 'ctrl',
    components: cellRendererDefinitions,
    defaultColDef: {
      sortable: true,
      filter: true,
      resizable: true,
    },
    onGridReady: () => this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(_ => this.applyFilterDevice(this.gridOptionsDevice)),
  };

  applyFilterDevice(gridOptions: GridOptions): void {
    const filteredDeviceName = this.route.snapshot.queryParams.deviceName;

    if (filteredDeviceName) {
      let nameFilter = gridOptions.api.getFilterInstance('Name');
      if (!nameFilter) {
        nameFilter = gridOptions.api.getFilterInstance('Name_1');
      }

      if (nameFilter) {
        nameFilter.setModel({
          type: 'contains',
          filter: filteredDeviceName
        });
      }
    } else {
      gridOptions.api.setFilterModel(null);
    }
    gridOptions.api.onFilterChanged();
  }

  updateColumnsDevice(fields: DataFieldDto[], gridOptions: GridOptions, canEdit: boolean) {
    const fieldGroups = this.columnHelper.createColumns(fields, 'Device', canEdit);

    // double-click on first column navigates to interfaces
    const firstColumn = fieldGroups[0].children[0] as ColDef;
    firstColumn.onCellDoubleClicked = (x) => this.detailClickedDevice(x);

    this.columnHelper.addColumnAfter(fieldGroups, 'Name', {
      headerName: ' ',
      field: 'detail',
      width: 50,
      onCellClicked: x => this.detailClickedDevice(x),
      cellRenderer: cellRendererNames.rightArrowButton,
      suppressSizeToFit: true,
      filter: false,
    });

    this.columnHelper.addColumnAfter(fieldGroups, 'detail', {
      headerName: ' ',
      field: 'on-page-filter',
      width: 50,
      onCellClicked: x => this.filterDevice(x),
      cellRenderer: cellRendererNames.filterButton,
      cellRendererParams: {
        filter: 'deviceName',
        value: this.route.snapshot.queryParams.deviceName,
      },
      suppressSizeToFit: true,
      filter: false,
    });

    if (canEdit) {
      // add edit icon
      this.columnHelper.addColumnAfter(fieldGroups, 'on-page-filter', {
        headerName: ' ',
        field: 'edit',
        width: 70,
        onCellClicked: x => this.editClickedDevice(x),
        cellRenderer: cellRendererNames.editButton,
        cellRendererParams: {dataTarget: '#editDeviceModal'},
        suppressSizeToFit: true,
        filter: false,
      });
    }

    gridOptions.columnDefs = fieldGroups;
    if (gridOptions.api) {
      gridOptions.api.setColumnDefs(fieldGroups);
      gridOptions.api.sizeColumnsToFit();
      gridOptions.columnApi.applyColumnState({state: [{colId: 'Name', sort: 'asc', sortIndex: 0}]});
    }

    this.applyFilterDevice(gridOptions);
  }

  async detailClickedDevice(event: CellClickedEvent) {
    const device = event.data as DeviceDto;
    if (device) {
      await this.router.navigate(['/interfaces'], {queryParams: {deviceName: device.properties.Name.value}});
    }
  }

  async filterDevice(event: CellClickedEvent) {
    const device = event.data as DeviceDto;

    if (device) {
      const deviceName = device.properties.Name.value;
      if (this.route.snapshot.queryParamMap.get('deviceName') === deviceName) {
        await this.router.navigate(['/devices'], {queryParams: {}});
        return;
      } else {
        await this.router.navigate(['/devices/detail'], {queryParams: {deviceName}});
      }
    }
    this.applyAllFilters();
  }

  editClickedDevice(event: CellClickedEvent) {
    const device = (event.data as DeviceDto);
    if (device) {
      this.dialogService.editDevice.initData(device);
      this.ngOnInit();
    }
  }

  ngOnInit() {
    this.apiData.callWhenReady((canEdit) => this.deviceService.getFields().pipe(map(fields => ({canEdit, fields}))))
      .pipe(takeUntil(this.destroy$))
      .subscribe(({canEdit, fields}) => this.updateColumnsDevice(fields, this.gridOptionsDevice, canEdit));
  }

  ngAfterViewInit() {
    if (this.gridOptionsDevice.api) {
      this.searchService.globalSearchQueryChanged$
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => this.goToDevicePage());

      this.modificationService.dataChanged$.pipe(takeUntil(this.destroy$)).subscribe(() => {
        this.updateGrids();
      });
    }
  }

  private applyAllFilters() {
    this.applyFilterDevice(this.gridOptionsDevice);
  }

  private updateGrids() {
    this.gridOptionsDevice.api.setDatasource(this.gridDataSourceDevice);
  }

  private async goToDevicePage() {
    await this.router.navigate(['/devices']);
  }
}
