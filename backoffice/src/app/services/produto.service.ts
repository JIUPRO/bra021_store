import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ProdutoDTO {
  id: string;
  nome: string;
  descricao?: string;
  descricaoCurta?: string;
  preco: number;
  precoPromocional?: number;
  valorFrete: number;
  prazoEntregaDias: number;
  imagemUrl?: string;
  quantidadeMinimaEstoque: number;
  destaque: boolean;
  peso: number;
  altura?: number;
  largura?: number;
  profundidade?: number;
  ativo: boolean;
  categoriaId: string;
  nomeCategoria: string;
}

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/produtos`;

  getAll(): Observable<ProdutoDTO[]> {
    return this.http.get<ProdutoDTO[]>(this.baseUrl);
  }

  getById(id: string): Observable<ProdutoDTO> {
    return this.http.get<ProdutoDTO>(`${this.baseUrl}/${id}`);
  }

  getByCategoriaId(categoriaId: string): Observable<ProdutoDTO[]> {
    return this.http.get<ProdutoDTO[]>(`${this.baseUrl}?categoriaId=${categoriaId}`);
  }

  create(dto: Partial<ProdutoDTO>) {
    return this.http.post<ProdutoDTO>(this.baseUrl, dto);
  }

  update(id: string, dto: Partial<ProdutoDTO>) {
    return this.http.put<ProdutoDTO>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
