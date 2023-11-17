import {Component, OnInit} from '@angular/core';
import {Observable} from 'rxjs';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import {ICellRendererParams} from 'ag-grid-community';
import {ActivatedRoute} from '@angular/router';
import {DestructibleComponent} from '../../common/destructible-component';
import {map} from 'rxjs/operators';
import {AsyncPipe} from '@angular/common';

@Component({
  selector: 'app-filter-button-cell-renderer',
  templateUrl: './filter-button-cell-renderer.component.html',
  styleUrls: ['./filter-button-cell-renderer.component.scss'],
  imports: [
    AsyncPipe
  ],
  standalone: true
})
export class FilterButtonCellRendererComponent extends DestructibleComponent implements OnInit, ICellRendererAngularComp {
  icon$: Observable<string>;
  private name: string;
  private filter: string;

  constructor(private route: ActivatedRoute) {
    super();
  }

  ngOnInit() {
    this.icon$ = this.route.queryParamMap.pipe(
      map(parms =>
        parms.get(this.filter) === this.name
          ? '../../../assets/filter-slash-regular.svg'
          : '../../../assets/filter-regular.svg'),
    );
  }

  agInit(params: ICellRendererParams): void {
    this.filter = (params as any).filter;
    this.name = (params as any).value;
  }

  refresh(params: any): boolean {
    return false;
  }
}
