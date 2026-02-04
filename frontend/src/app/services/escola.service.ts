import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Escola } from '../models/escola.model';

@Injectable({
  providedIn: 'root'
})
export class EscolaService {
  constructor(private api: ApiService) {}

  obterAtivas(): Observable<Escola[]> {
    return this.api.get<Escola[]>('escolas/ativas');
  }
}
