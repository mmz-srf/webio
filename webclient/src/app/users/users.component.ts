import {Component, Inject, OnInit} from '@angular/core';
import {BehaviorSubject, Observable} from 'rxjs';
import {DestructibleComponent} from '../common/destructible-component';
import {filter, map, takeUntil} from 'rxjs/operators';
import {MSAL_GUARD_CONFIG, MsalBroadcastService, MsalGuardConfiguration, MsalService} from '@azure/msal-angular';
import {InteractionStatus} from '@azure/msal-browser';
import {environment} from '../../environments/environment';
import {AsyncPipe, NgIf, NgOptimizedImage} from '@angular/common';


@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
  imports: [
    NgIf,
    AsyncPipe,
    NgOptimizedImage
  ],
  standalone: true
})
export class UsersComponent extends DestructibleComponent implements OnInit {
  loggedIn$: Observable<boolean> = new BehaviorSubject(false);

  constructor(
    @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration,
    private broadcastService: MsalBroadcastService,
    private authService: MsalService) {
    super();
  }

  ngOnInit() {
    this.loggedIn$ = this.broadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None),
        map(() => this.authService.instance.getAllAccounts().length > 0),
        takeUntil(this.destroy$));
  }

  logout() {
    this.authService.logoutRedirect({
      postLogoutRedirectUri: environment.appRootUrl
    });
  }
}
