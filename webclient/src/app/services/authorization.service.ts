import {Injectable} from '@angular/core';
import {ReplaySubject} from 'rxjs';
import {MsalBroadcastService} from '@azure/msal-angular';
import {filter, mergeMap} from 'rxjs/operators';
import {InteractionStatus} from '@azure/msal-browser';
import {AuthorizationApi} from '../data/services/authorization-api';

@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {
  private canEditSubject = new ReplaySubject<boolean>();
  canEdit$;

  constructor(
    private authorizationApi: AuthorizationApi,
    private broadcastService: MsalBroadcastService) {
    this.broadcastService.inProgress$
      .pipe(filter((status: InteractionStatus) => status === InteractionStatus.None),
        mergeMap(() => this.authorizationApi.writeAccess())
      ).subscribe(this.canEditSubject);
    this.canEdit$ = this.canEditSubject.asObservable();
  }
}
