import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UploadService } from './upload.service';

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
  imagemKey?: string;
  quantidadeMinimaEstoque: number;
  destaque: boolean;
  ativo: boolean;
  categoriaId: string;
  nomeCategoria: string;
}

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private http = inject(HttpClient);
  private uploadService = inject(UploadService);
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

  uploadImagem(id: string, file: File) {
    return this.uploadService.uploadFile(`produtos/${id}/imagem`, file);
  }

  removerImagem(id: string) {
    return this.uploadService.delete(`produtos/${id}/imagem`);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
