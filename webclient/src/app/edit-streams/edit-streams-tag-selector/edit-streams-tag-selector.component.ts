import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {Observable} from 'rxjs';
import {TagSelection} from '../edit-streams.component';
import {ApiDataService} from '../../services/api-data.service';
import {StreamService} from '../../services/stream.service';
import {map, mergeMap} from 'rxjs/operators';
import {StreamDto} from '../../data/models/stream-dto';
import {TagDto} from '../../data/models/tag-dto';
import {AsyncPipe} from '@angular/common';

@Component({
  selector: 'app-edit-streams-tag-selector',
  templateUrl: './edit-streams-tag-selector.component.html',
  styleUrls: ['./edit-streams-tag-selector.component.scss'],
  imports: [
    AsyncPipe
  ],
  standalone: true
})
export class EditStreamsTagSelectorComponent implements OnInit {
  @Input()
  public streams$: Observable<StreamDto[]>;

  @Output()
  public tagChanged = new EventEmitter<{ tagName: string, newVal: boolean }>();

  public streamsHaveTagsInCommon$: Observable<boolean>;

  public tags$: Observable<TagSelection[]>;
  private allTags$: Observable<TagSelection[]>;

  constructor(private apiData: ApiDataService, private streamService: StreamService) {
  }

  ngOnInit() {
    this.allTags$ = this.apiData
      .callWhenReady(() => this.streamService.getTags())
      .pipe(map(tags => this.toTagSelections(tags)));

    this.tags$ = this.getAllCommonTags$();
    this.streamsHaveTagsInCommon$ = this.tags$.pipe(map(tags => tags && tags.length > 0));
  }

  private toTagSelections(tags: TagDto[]) {
    return tags.map<TagSelection>(tag => new TagSelection(tag.name, tag.streamType));
  }

  private getAllCommonTags$() {
    return this.allTags$.pipe(
      mergeMap(allTags => this.streams$.pipe(map(streams => streams.map(s => allTags.filter(t => t.streamType === s.type))))),
      map(streamTags => this.intersect(streamTags)),
    );
  }

  private intersect<T>(arrays: T[][]): T[] {
    if (!arrays || arrays.length === 0) {
      return null;
    }
    if (arrays.length === 1) {
      return arrays[0];
    }

    const filter = (a, b) => {
      const set = new Set(b);
      return a.filter(val => set.has(val));
    };
    return arrays.reduce(filter);
  }

  allStreamsHave$(tag: TagSelection): Observable<boolean> {
    return this.streams$
      .pipe(map(streams => streams.every(s => s.properties.Tags.value.includes(tag.name))));
  }

  change(name: string, $event) {
    this.tagChanged.emit({tagName: name, newVal: $event.target.checked});
  }
}
