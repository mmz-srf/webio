import {Component, OnInit} from '@angular/core';
import {BehaviorSubject, Observable, Subject} from 'rxjs';
import {DialogService} from '../services/dialog.service';
import {ModificationService} from '../services/modification.service';
import {StreamService} from '../services/stream.service';
import {DestructibleComponent} from '../common/destructible-component';
import {setTag} from '../common/stream-helper';
import {DataFieldDto} from '../data/models/data-field-dto';
import {StreamDto} from '../data/models/stream-dto';
import {EditStreamsTagSelectorComponent} from './edit-streams-tag-selector/edit-streams-tag-selector.component';
import {AsyncPipe, NgForOf, NgIf} from '@angular/common';
import {EditFieldsComponent} from '../edit-fields/edit-fields.component';

export class TagSelection {
  constructor(
    public name: string,
    public streamType: string) {
  }

  public isSelected = false;
}

@Component({
  selector: 'app-edit-streams',
  templateUrl: './edit-streams.component.html',
  styleUrls: ['./edit-streams.component.scss'],
  imports: [
    EditStreamsTagSelectorComponent,
    NgForOf,
    EditFieldsComponent,
    NgIf,
    AsyncPipe
  ],
  standalone: true
})
export class EditStreamsComponent extends DestructibleComponent implements OnInit {
  public streams: StreamDto[] = [];
  public streams$ = new BehaviorSubject<StreamDto[]>([]);
  public header: string;

  private dataChangedSubject = new Subject<boolean>();
  public dataChanged$: Observable<boolean> = this.dataChangedSubject.asObservable();
  private tagUpdates: Map<string, boolean>;
  private fieldUpdates: Map<DataFieldDto, string>;

  constructor(
    public streamService: StreamService,
    private modificationService: ModificationService,
    private dialogService: DialogService) {
    super();
  }


  onUpdateStream() {
    this.streams.forEach(stream => {
      this.fieldUpdates.forEach((newVal, field) => {
        this.modificationService.changeValue(
          field.key,
          newVal,
          stream.id,
          stream.deviceId,
          'Stream'
        );
      });


      if (this.tagUpdates.size > 0) {
        this.tagUpdates.forEach((newVal, tagName) => setTag(stream, tagName, newVal));

        this.modificationService.changeValue(
          'Tags',
          stream.properties.Tags.value,
          stream.id,
          stream.deviceId,
          'Stream'
        );
      }
    });



    this.streams = [];
    this.dataChangedSubject.next(true);
  }

  initData(streams: StreamDto[]) {
    this.fieldUpdates = new Map<DataFieldDto, string>();
    this.tagUpdates = new Map<string, boolean>();

    this.streams = streams;
    this.streams$.next(streams);

    this.header = streams.length === 1 ? 'Update Stream' : 'Update Streams';
  }

  cancel() {
  }

  ngOnInit() {
    this.dialogService.editStream = this;
  }

  addFieldUpdate(field: DataFieldDto, newVal: string) {
    this.fieldUpdates.set(field, newVal);
  }

  addTagUpdate(tagName: string, newVal: boolean) {
    this.tagUpdates.set(tagName, newVal);
  }
}
