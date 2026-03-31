import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CotacaoFreteRequest, CotacaoFreteResponse } from '../models/frete.model';

@Injectable({
  providedIn: 'root'
})
export class FreteService {
  constructor(private api: ApiService) {}

  cotar(dados: CotacaoFreteRequest): Observable<CotacaoFreteResponse> {
    return this.api.post<CotacaoFreteResponse>('frete/cotar', dados);
  }
}
