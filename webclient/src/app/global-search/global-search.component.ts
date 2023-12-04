import {Component, OnInit} from '@angular/core';
import {debounceTime, distinctUntilChanged, takeUntil} from 'rxjs/operators';
import {SearchService} from '../services/search.service';
import {Subject} from 'rxjs';
import {DestructibleComponent} from '../common/destructible-component';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-globalsearch',
  templateUrl: './global-search.component.html',
  styleUrls: ['./global-search.component.scss'],
  imports: [
    FormsModule
  ],
  standalone: true
})
export class GlobalSearchComponent extends DestructibleComponent implements OnInit {

  constructor(private search: SearchService) {
    super();
  }

  public modelChanged: Subject<string> = new Subject<string>();
  public globalSearchQuery = '';

  changed(text: string) {
    this.modelChanged.next(text);
  }

  ngOnInit() {
    this.modelChanged.pipe(
      debounceTime(1000),
      distinctUntilChanged(),
      takeUntil(this.destroy$))
      .subscribe(model => {
        if (model) {
          this.search.setGlobalSearch(model);
        } else {
          this.search.setGlobalSearch('');
        }
      });
  }
}
