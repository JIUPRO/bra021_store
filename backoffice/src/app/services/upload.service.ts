import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UploadService {
  private http = inject(HttpClient);

  uploadFile(relativeUrl: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${environment.apiUrl}/${relativeUrl}`, formData);
  }

  delete(relativeUrl: string): Observable<any> {
    return this.http.delete(`${environment.apiUrl}/${relativeUrl}`);
  }
}
