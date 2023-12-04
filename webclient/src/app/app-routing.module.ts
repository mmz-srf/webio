import {NgModule} from '@angular/core';
import {Routes, RouterModule} from '@angular/router';
import {DevicesComponent} from './devices/devices.component';
import {InterfacesComponent} from './interfaces/interfaces.component';
import {StreamsComponent} from './streams/streams.component';
import {DeviceDetailComponent} from './device-detail/device-detail.component';
import {MsalGuard} from '@azure/msal-angular';


const routes: Routes = [
  {
    path: '',
    component: DevicesComponent,
    canActivate: [MsalGuard]
  },
  {
    path: 'devices',
    component: DevicesComponent,
    canActivate: [MsalGuard]
  },
  {
    path: 'interfaces',
    component: InterfacesComponent,
    canActivate: [MsalGuard]
  },
  {
    path: 'streams',
    component: StreamsComponent,
    canActivate: [MsalGuard]
  },
  {
    path: 'devices/detail',
    component: DeviceDetailComponent,
    canActivate: [MsalGuard]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    onSameUrlNavigation: 'reload'
  })],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
