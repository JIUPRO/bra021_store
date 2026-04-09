import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CepResponse {
  cep: string;
  state: string;
  city: string;
  neighborhood: string;
  street: string;
  service?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CepService {
  constructor(private http: HttpClient) {}

  consultarCep(cep: string): Observable<CepResponse> {
    const cepLimpo = (cep || '').replace(/\D/g, '');
    return this.http.get<CepResponse>(`https://brasilapi.com.br/api/cep/v1/${cepLimpo}`);
  }
}
