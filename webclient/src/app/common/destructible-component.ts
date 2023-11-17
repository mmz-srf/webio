import {Injectable, OnDestroy} from '@angular/core';
import {Subject} from 'rxjs';

@Injectable()
export abstract class DestructibleComponent implements OnDestroy {
  private destroySubject$ = new Subject<void>();

  protected get destroy$() {
    return this.destroySubject$.asObservable();
  }

  ngOnDestroy(): void {
    this.destroySubject$.next();
    this.destroySubject$.complete();
  }
}
