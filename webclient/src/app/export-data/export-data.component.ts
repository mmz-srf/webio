import {Component, DoCheck, OnInit} from '@angular/core';
import {ExportService, ExportType} from '../services/export.service';
import {DialogService} from '../services/dialog.service';
import {takeUntil} from 'rxjs/operators';
import {DestructibleComponent} from '../common/destructible-component';
import {ApiDataService} from '../services/api-data.service';
import {objectToFilter} from '../common/utils';
import {ToastrService} from 'ngx-toastr';
import {FormsModule} from '@angular/forms';

declare var $: any;

@Component({
  selector: 'app-export-data',
  templateUrl: './export-data.component.html',
  styleUrls: ['./export-data.component.scss'],
  imports: [
    FormsModule
  ],
  standalone: true
})
export class ExportDataComponent extends DestructibleComponent implements OnInit, DoCheck {

  public exportTargets = [];
  public export;

  public exportTypes: ExportType[];
  public selectedExportTargetName: string;
  public exportSelections = ['all Devices', 'current Filter', 'current selected device(s)'];
  public exportSelection: string;
  public selectedExportSelection: string;

  constructor(
    private apiDataService: ApiDataService,
    private exportService: ExportService,
    private dialogService: DialogService,
    private toastr: ToastrService) {
    super();
  }


  selectExportTarget(exportTarget: string): void {
    this.selectedExportTargetName = exportTarget;
  }

  selectExportSelection(exportSelection: string): void {
    this.exportSelection = exportSelection;
  }

  getFilters() {
    const values = this.dialogService.gridOptions.api.getFilterModel();
    return objectToFilter(values);
  }

  async onStartExport(): Promise<void> {
    const target = this.selectedExportTargetName;
    const filters = this.getFilters();
    const selectedDeviceIds: string[] = this.getSelectedDeviceIds();
    this.toastr.info('Export started!');
    await this.exportService.export(target, this.exportAll(), selectedDeviceIds, filters);
  }

  exportAll(): boolean {
    return this.exportSelection === this.exportSelections[0];
  }

  getSelectedDeviceIds(): string[] {
    if (this.exportSelection !== this.exportSelections[2]) {
      return [];
    }

    const rows = this.dialogService.gridOptions.api.getSelectedRows();
    return rows.map(r => r.deviceId);
  }

  cancel() {
  }

  ngDoCheck(): void {
    if (!($('#exportDataModal').hasClass('show'))) {
      this.selectedExportSelection = '';
    }
  }

  ngOnInit() {
    this.apiDataService
      .callWhenReady(() => this.exportService.getExportTypes())
      .pipe(takeUntil(this.destroy$))
      .subscribe(x =>
        x.map(y => {
          this.exportTargets.push(y.displayName);
          this.exportTypes = x;
        }), err => {
        throw err;
      });
  }
}
