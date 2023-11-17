import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Params, Router} from '@angular/router';
import {Location} from '@angular/common';
import {environment} from '../environments/environment';
import {filter, map, takeUntil} from 'rxjs/operators';
import {Observable} from 'rxjs';
import {MsalBroadcastService} from '@azure/msal-angular';
import {InteractionStatus} from '@azure/msal-browser';
import {DestructibleComponent} from './common/destructible-component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent extends DestructibleComponent implements OnInit {
  public currentInterfaceFilter$: Observable<Params>;
  public currentStreamsFilter$: Observable<Params>;

  authFinished$: Observable<boolean>;

  constructor(
    private broadcastService: MsalBroadcastService,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location) {
    super();
    if (['', '/'].includes(location.path())) {
      router.navigate(['/devices']);
    }
  }

  title = environment.title;

  ngOnInit() {
    this.authFinished$ = this.broadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None),
        map(() => true),
        takeUntil(this.destroy$));

    this.currentInterfaceFilter$ = this.route.queryParams.pipe(map(p => {
      return {deviceName: p.deviceName};
    }));
    this.currentStreamsFilter$ = this.route.queryParams;
  }
}
