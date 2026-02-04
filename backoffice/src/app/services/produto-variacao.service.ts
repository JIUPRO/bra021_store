import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ProdutoTamanhoDTO {
  id?: string;
  produtoId: string;
  tamanho: string;
  ativo: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ProdutoTamanhoService {
  private apiUrl = `${environment.apiUrl}/tamanhos`;

  constructor(private http: HttpClient) { }

  getByProdutoId(produtoId: string): Observable<ProdutoTamanhoDTO[]> {
    return this.http.get<ProdutoTamanhoDTO[]>(`${this.apiUrl}/produto/${produtoId}`);
  }

  getById(id: string): Observable<ProdutoTamanhoDTO> {
    return this.http.get<ProdutoTamanhoDTO>(`${this.apiUrl}/${id}`);
  }

  create(tamanho: ProdutoTamanhoDTO): Observable<ProdutoTamanhoDTO> {
    return this.http.post<ProdutoTamanhoDTO>(this.apiUrl, tamanho);
  }

  update(id: string, tamanho: ProdutoTamanhoDTO): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, tamanho);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
