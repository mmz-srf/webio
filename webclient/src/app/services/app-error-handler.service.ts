import {Injectable, ErrorHandler, Injector, Inject} from '@angular/core';
import {ToastrService} from 'ngx-toastr';
import {HttpErrorResponse} from '@angular/common/http';


@Injectable()
export class AppErrorHandlerService extends ErrorHandler {
  constructor(@Inject(Injector) private injector: Injector) {
    super();
  }

  // Need to get ToastrService from injector rather than constructor injection to avoid cyclic dependency error
  private get toastrService(): ToastrService {
    return this.injector.get(ToastrService);
  }

  public handleError(error: any): void {

    let errorMessage: string;
    if (error instanceof HttpErrorResponse) {
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.statusText} `;
    } else if (error.rejection && error.rejection.status != null) {
      errorMessage = `Message: ${error.rejection.error} `;
    } else {
      errorMessage = `${error}`;
    }
    this.toastrService.error(errorMessage, 'Error');
    super.handleError(error);
  }
}
