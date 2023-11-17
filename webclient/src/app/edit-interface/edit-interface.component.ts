import {Component, OnInit} from '@angular/core';
import {async, BehaviorSubject, Observable, Subject} from 'rxjs';
import {ModificationService} from '../services/modification.service';
import {DialogService} from '../services/dialog.service';
import {InterfaceService} from '../services/interface.service';
import {DataFieldDto} from '../data/models/data-field-dto';
import {InterfaceDto} from '../data/models/interface-dto';
import {EditFieldsComponent} from '../edit-fields/edit-fields.component';
import {AsyncPipe, NgForOf} from '@angular/common';

@Component({
  selector: 'app-edit-interface',
  templateUrl: './edit-interface.component.html',
  styleUrls: ['./edit-interface.component.scss'],
  imports: [
    EditFieldsComponent,
    NgForOf,
    AsyncPipe
  ],
  standalone: true
})
export class EditInterfaceComponent implements OnInit {
  private fieldUpdates: Map<DataFieldDto, string>;
  private interfaces: InterfaceDto[];
  interfaces$ = new BehaviorSubject<InterfaceDto[]>([]);
  header: string;

  private dataChangedSubject = new Subject<boolean>();
  public dataChanged$: Observable<boolean> = this.dataChangedSubject.asObservable();

  constructor(
    public interfaceService: InterfaceService,
    private modificationService: ModificationService,
    private dialogService: DialogService
  ) {
  }

  ngOnInit() {
    this.dialogService.editInterface = this;
  }

  onUpdateInterface() {
    this.interfaces.forEach(iface => {
      this.fieldUpdates.forEach((newVal, field) => {
        this.modificationService.changeValue(
          field.key,
          newVal,
          iface.id,
          iface.deviceId,
          'Interface'
        );
      });
    });

    this.dataChangedSubject.next(true);
  }

  initData(interfaces: InterfaceDto[]) {
    this.fieldUpdates = new Map<DataFieldDto, string>();
    this.interfaces = interfaces;
    this.interfaces$.next(interfaces);

    this.header = interfaces.length === 1 ? 'Update Interface' : 'Update Interfaces';
  }

  cancel() {
  }

  addFieldUpdate(field: DataFieldDto, newVal: string) {
    this.fieldUpdates.set(field, newVal);
  }

  protected readonly async = async;
}
