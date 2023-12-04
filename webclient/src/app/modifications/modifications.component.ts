import {Component, OnInit} from '@angular/core';
import {ModificationService} from '../services/modification.service';
import {takeUntil} from 'rxjs/operators';
import {DestructibleComponent} from '../common/destructible-component';

@Component({
    selector: 'app-modifications',
    templateUrl: './modifications.component.html',
    styleUrls: ['./modifications.component.scss'],
    standalone: true
})
export class ModificationsComponent extends DestructibleComponent implements OnInit {

  constructor(private modificationsService: ModificationService) {
    super();
  }

  public pendingModificationsCount = 0;

  onSave() {
    this.modificationsService.saveChanges();
  }

  onDiscard() {
    this.modificationsService.discardChanges();
  }

  ngOnInit() {
    this.modificationsService.pending$
      .pipe(takeUntil(this.destroy$))
      .subscribe(x => this.pendingModificationsCount = x);
  }
}
