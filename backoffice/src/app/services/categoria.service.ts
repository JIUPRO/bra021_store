import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UploadService } from './upload.service';

export interface CategoriaDTO {
  id: string;
  nome: string;
  descricao?: string;
  imagemUrl?: string;
  imagemKey?: string;
  ordemExibicao?: number;
  quantidadeProdutos?: number;
}

@Injectable({ providedIn: 'root' })
export class CategoriaService {
  private http = inject(HttpClient);
  private uploadService = inject(UploadService);
  private baseUrl = `${environment.apiUrl}/categorias`;

  getAll(): Observable<CategoriaDTO[]> {
    return this.http.get<CategoriaDTO[]>(this.baseUrl);
  }

  getById(id: string): Observable<CategoriaDTO> {
    return this.http.get<CategoriaDTO>(`${this.baseUrl}/${id}`);
  }

  create(dto: Partial<CategoriaDTO>) {
    return this.http.post<CategoriaDTO>(this.baseUrl, dto);
  }

  update(id: string, dto: Partial<CategoriaDTO>) {
    return this.http.put<CategoriaDTO>(`${this.baseUrl}/${id}`, dto);
  }

  uploadImagem(id: string, file: File) {
    return this.uploadService.uploadFile(`categorias/${id}/imagem`, file);
  }

  removerImagem(id: string) {
    return this.uploadService.delete(`categorias/${id}/imagem`);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
