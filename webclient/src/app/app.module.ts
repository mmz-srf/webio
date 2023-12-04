import {BrowserModule} from '@angular/platform-browser';
import {ErrorHandler, NgModule} from '@angular/core';
import {FormsModule} from '@angular/forms';

import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {DevicesComponent} from './devices/devices.component';
import {InterfacesComponent} from './interfaces/interfaces.component';
import {StreamsComponent} from './streams/streams.component';
import {ApiModule} from './data/api.module';
import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import {DeviceService} from './services/device.service';
import {InterfaceService} from './services/interface.service';
import {ModificationService} from './services/modification.service';
import {DialogService} from './services/dialog.service';
import {ModificationsComponent} from './modifications/modifications.component';
import {UsersComponent} from './users/users.component';
import {ExportDataComponent} from './export-data/export-data.component';
import {EditDeviceComponent} from './edit-device/edit-device.component';
import {ExportService} from './services/export.service';
import {StreamService} from './services/stream.service';
import {GlobalSearchComponent} from './global-search/global-search.component';
import {SearchService} from './services/search.service';
import {EditStreamsComponent} from './edit-streams/edit-streams.component';
import {ColumnHelperService} from './services/column-helper.service';
import {DeviceDetailComponent} from './device-detail/device-detail.component';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {ToastrModule} from 'ngx-toastr';
import {AppErrorHandlerService} from './services/app-error-handler.service';
import {AddDeviceComponent} from './add-device/add-device.component';
import {FilterButtonCellRendererComponent} from './cell-renderers/filter-button-cell-renderer/filter-button-cell-renderer.component';
import {EditButtonCellRendererComponent} from './cell-renderers/edit-button-cell-renderer/edit-button-cell-renderer.component';
import {
  RightArrowButtonCellRendererComponent
} from './cell-renderers/right-arrow-button-cell-renderer/right-arrow-button-cell-renderer.component';
import {IpAddressCellEditorComponent} from './cell-renderers/ip-address-cell-editor/ip-address-cell-editor.component';
import {MaxLengthCellEditorComponent} from './cell-renderers/max-length-cell-editor/max-length-cell-editor.component';
import {PlaceholderCellEditorComponent} from './cell-renderers/placeholder-cell-editor/placeholder-cell-editor.component';
import {
  MSAL_GUARD_CONFIG,
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalGuard,
  MsalGuardConfiguration,
  MsalInterceptor,
  MsalInterceptorConfiguration,
  MsalModule,
  MsalRedirectComponent,
  MsalService
} from '@azure/msal-angular';
import {InteractionType, IPublicClientApplication, PublicClientApplication} from '@azure/msal-browser';
import {msalConfig, protectedResources} from './auth-config';
import {EditFieldsComponent} from './edit-fields/edit-fields.component';
import {EditStreamsTagSelectorComponent} from './edit-streams/edit-streams-tag-selector/edit-streams-tag-selector.component';
import {EditInterfaceComponent} from './edit-interface/edit-interface.component';
import {environment} from '../environments/environment';
import {IsDuplicateCellEditorComponent} from './cell-renderers/is-duplicate-cell-editor/is-duplicate-cell-editor.component';
import {AgGridModule} from 'ag-grid-angular';
import {ConfirmBoxConfigModule, NgxAwesomePopupModule} from '@costlydeveloper/ngx-awesome-popup';
import {NgSelectModule} from "@ng-select/ng-select";
import { SelectionCellEditorComponent } from './cell-renderers/selection-cell-editor/selection-cell-editor.component';

export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication(msalConfig);
}

export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap: protectedResources.webio
  };
}

export function MSALGuardConfigFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: {
      scopes: ['user.read']
    }
  };
}

@NgModule({
    declarations: [AppComponent],
    imports: [
        ApiModule.forRoot({ rootUrl: `${environment.appRootUrl}` }),
        BrowserModule,
        FormsModule,
        AppRoutingModule,
        HttpClientModule,
        AgGridModule,
        BrowserAnimationsModule,
        NgxAwesomePopupModule.forRoot(),
        ConfirmBoxConfigModule.forRoot(),
        ToastrModule.forRoot({
            positionClass: 'toast-bottom-right',
            disableTimeOut: true
        }),
        MsalModule,
        AgGridModule,
        NgSelectModule,
        DevicesComponent,
        InterfacesComponent,
        StreamsComponent,
        ModificationsComponent,
        UsersComponent,
        AddDeviceComponent,
        ExportDataComponent,
        EditDeviceComponent,
        GlobalSearchComponent,
        EditStreamsComponent,
        DeviceDetailComponent,
        AddDeviceComponent,
        FilterButtonCellRendererComponent,
        EditButtonCellRendererComponent,
        RightArrowButtonCellRendererComponent,
        IpAddressCellEditorComponent,
        MaxLengthCellEditorComponent,
        PlaceholderCellEditorComponent,
        EditFieldsComponent,
        EditFieldsComponent,
        EditStreamsTagSelectorComponent,
        EditInterfaceComponent,
        IsDuplicateCellEditorComponent,
        SelectionCellEditorComponent,
    ],
    providers: [
        DeviceService,
        InterfaceService,
        StreamService,
        ExportService,
        ModificationService,
        DialogService,
        SearchService,
        AppErrorHandlerService,
        { provide: ErrorHandler, useClass: AppErrorHandlerService },
        ColumnHelperService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: MsalInterceptor,
            multi: true
        },
        {
            provide: MSAL_INSTANCE,
            useFactory: MSALInstanceFactory
        },
        {
            provide: MSAL_GUARD_CONFIG,
            useFactory: MSALGuardConfigFactory
        },
        {
            provide: MSAL_INTERCEPTOR_CONFIG,
            useFactory: MSALInterceptorConfigFactory
        },
        MsalGuard,
        MsalService
    ],
    bootstrap: [AppComponent, MsalRedirectComponent]
})
export class AppModule {
}
