import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {CellClickedEvent, ColDef, ColumnState} from 'ag-grid-community';
import {GenericDataSource} from '../common/generic-data-source';
import {ColumnHelperService} from '../services/column-helper.service';
import {DeviceService} from '../services/device.service';
import {DialogService} from '../services/dialog.service';
import {ModificationService} from '../services/modification.service';
import {SearchService} from '../services/search.service';
import {map, takeUntil} from 'rxjs/operators';
import {GridDataComponent} from '../common/grid-data-component';
import {cellRendererNames} from '../cell-renderers/cell-renderer-definitions';
import {ApiDataService} from '../services/api-data.service';
import {FilterService} from '../services/filter.service';
import {DataFieldDto} from '../data/models/data-field-dto';
import {DeviceDto} from '../data/models';
import {AgGridModule} from 'ag-grid-angular';

@Component({
  selector: 'app-devices',
  templateUrl: './devices.component.html',
  styleUrls: ['./devices.component.scss'],
  imports: [
    AgGridModule
  ],
  standalone: true
})
export class DevicesComponent extends GridDataComponent<DeviceDto> implements OnInit {
  constructor(
    private deviceService: DeviceService,
    private modificationService: ModificationService,
    private apiData: ApiDataService,
    private searchService: SearchService,
    private dialogService: DialogService,
    private router: Router,
    private route: ActivatedRoute,
    private filters: FilterService,
    private columnHelper: ColumnHelperService) {
    super(
      modificationService,
      searchService,
      new GenericDataSource<DeviceDto>(deviceService));

    this.gridOptions.onGridReady = () => this.filters.apply(this.gridOptions.api);
  }

  updateColumns(fields: DataFieldDto[], canEdit: boolean) {
    const fieldGroups = this.columnHelper.createColumns(fields, 'Device', canEdit);

    // double-click on first column navigates to interfaces
    const firstColumn = fieldGroups[0].children[0] as ColDef;
    firstColumn.onCellDoubleClicked = (x) => this.detailClicked(x);

    this.columnHelper.addColumnAfter(fieldGroups, 'Name', {
      headerName: ' ',
      field: 'detail',
      width: 50,
      onCellClicked: x => this.detailClicked(x),
      cellRenderer: cellRendererNames.rightArrowButton,
      suppressSizeToFit: true,
      filter: false,
    });

    this.columnHelper.addColumnAfter(fieldGroups, 'Name', {
      headerName: ' ',
      field: 'deviceDetail',
      width: 50,
      onCellClicked: x => this.detailDeviceClicked(x),
      cellRenderer: () => '<img alt="Detail" class="detailicon" src="../../assets/zoom-in.svg" />',
      suppressSizeToFit: true,
      filter: false,
    });

    if (canEdit) {
      // add edit icon
      this.columnHelper.addColumnAfter(fieldGroups, 'detail', {
        headerName: ' ',
        field: 'edit',
        width: 70,
        onCellClicked: x => this.editClicked(x),
        cellRenderer: cellRendererNames.editButton,
        cellRendererParams: {dataTarget: '#editDeviceModal'},
        suppressSizeToFit: true,
        filter: false,
      });
    }

    const defaultSortModel: ColumnState[] = [
      {
        colId: 'Name',
        sort: 'asc',
        sortIndex: 0
      }
    ];

    // update grid
    this.gridOptions.columnDefs = fieldGroups;
    if (this.gridOptions.api) {
      this.gridOptions.api.setColumnDefs(fieldGroups);
      this.gridOptions.api.sizeColumnsToFit();
      this.gridOptions.columnApi.applyColumnState({state: defaultSortModel});
    }
    this.dialogService.gridOptions = this.gridOptions;
  }

  async detailClicked(event: CellClickedEvent): Promise<void> {
    const device = event.data as DeviceDto;
    if (device) {
      await this.router.navigate(['/interfaces'], {queryParams: {deviceName: device.properties.Name.value}});
    }
  }

  editClicked(event: CellClickedEvent) {
    const device = (event.data as DeviceDto);
    if (device) {
      this.dialogService.editDevice.initData(device);
      this.ngOnInit();
    }
  }

  async detailDeviceClicked(event: CellClickedEvent) {
    const device = event.data as DeviceDto;
    if (device) {
      await this.router.navigate(['/devices/detail'], {queryParams: {deviceName: device.properties.Name.value}});
    }
  }

  ngOnInit() {
    super.onInit();
    this.apiData.callWhenReady((canEdit) => this.deviceService.getFields().pipe(map(fields => ({canEdit, fields}))))
      .pipe(takeUntil(this.destroy$))
      .subscribe(({canEdit, fields}) => this.updateColumns(fields, canEdit));
  }
}
