import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';


export interface ExportType {
  name: string;
  displayName: string;
}

@Injectable()
export class ExportService {

  constructor(private http: HttpClient) {
  }

  getExportTypes(): Observable<ExportType[]> {
    return this.http.get<ExportType[]>('/api/export/types');
  }

  public export(exportTargetName: string, all: boolean, selectedDeviceIds: string[], filters: any): Promise<void> {
    const args = {
      exportTargetName,
      all,
      selectedDeviceIds,
      filters
    };

    const newTab = window.open('', '_self');
    return this.http.post('/api/export', args, {responseType: 'text'})
      .toPromise()
      .then(result => {
        newTab.location.href = '/api/export/downloadfile/' + result;
      })
      .catch(exp => {
        console.log(exp);
      });
  }
}
