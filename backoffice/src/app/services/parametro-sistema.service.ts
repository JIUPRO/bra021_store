import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ParametroSistema, AtualizarParametro, CriarParametro } from '../models/parametro-sistema.model';

@Injectable({ providedIn: 'root' })
export class ParametroSistemaService {
  private apiUrl = `${environment.apiUrl}/parametrossistema`;
  private http = inject(HttpClient);

  obterTodos(): Observable<ParametroSistema[]> {
    return this.http.get<ParametroSistema[]>(this.apiUrl);
  }

  obterPorChave(chave: string): Observable<ParametroSistema> {
    return this.http.get<ParametroSistema>(`${this.apiUrl}/chave/${chave}`);
  }

  criar(dados: CriarParametro): Observable<ParametroSistema> {
    return this.http.post<ParametroSistema>(this.apiUrl, dados);
  }

  atualizar(id: string, dados: AtualizarParametro): Observable<ParametroSistema> {
    console.log('Enviando PUT para:', `${this.apiUrl}/${id}`, 'dados:', dados);
    return this.http.put<ParametroSistema>(`${this.apiUrl}/${id}`, dados);
  }
}
