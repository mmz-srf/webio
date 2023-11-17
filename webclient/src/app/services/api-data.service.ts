import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {filter, mergeMap} from 'rxjs/operators';
import {InteractionStatus} from '@azure/msal-browser';
import {MsalBroadcastService} from '@azure/msal-angular';
import {AuthorizationService} from './authorization.service';

@Injectable({
  providedIn: 'root'
})
export class ApiDataService {
  constructor(
    protected broadcastService: MsalBroadcastService,
    protected authorizationService: AuthorizationService,
  ) { }

  public callWhenReady<TRes>(project: (canEdit: boolean) => Observable<TRes>): Observable<TRes> {
    return this.broadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None),
        mergeMap(() => this.authorizationService.canEdit$),
        mergeMap(project));
  }
}
