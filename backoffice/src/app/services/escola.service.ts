import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Escola, CriarEscola, AtualizarEscola } from '../models/escola.model';

@Injectable({
  providedIn: 'root'
})
export class EscolaService {
  private apiUrl = `${environment.apiUrl}/escolas`;
  private http = inject(HttpClient);

  obterTodas(): Observable<Escola[]> {
    return this.http.get<Escola[]>(this.apiUrl);
  }

  obterAtivas(): Observable<Escola[]> {
    return this.http.get<Escola[]>(`${this.apiUrl}/ativas`);
  }

  obterPorId(id: string): Observable<Escola> {
    return this.http.get<Escola>(`${this.apiUrl}/${id}`);
  }

  criar(dados: CriarEscola): Observable<Escola> {
    return this.http.post<Escola>(this.apiUrl, dados);
  }

  atualizar(id: string, dados: AtualizarEscola): Observable<Escola> {
    return this.http.put<Escola>(`${this.apiUrl}/${id}`, dados);
  }

  remover(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
