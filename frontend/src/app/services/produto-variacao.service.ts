import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ProdutoVariacao } from '../models/produto.model';

@Injectable({
  providedIn: 'root'
})
export class ProdutoVariacaoService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/tamanhos`;

  getByProdutoId(produtoId: string): Observable<ProdutoVariacao[]> {
    return this.http.get<ProdutoVariacao[]>(`${this.apiUrl}/produto/${produtoId}`);
  }

  getById(id: string): Observable<ProdutoVariacao> {
    return this.http.get<ProdutoVariacao>(`${this.apiUrl}/${id}`);
  }

  create(variacao: ProdutoVariacao): Observable<ProdutoVariacao> {
    return this.http.post<ProdutoVariacao>(this.apiUrl, variacao);
  }

  update(id: string, variacao: ProdutoVariacao): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, variacao);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
