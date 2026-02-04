import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ClienteDTO {
  id: string;
  nome: string;
  email: string;
  telefone?: string;
  cpf?: string;
  dataNascimento?: string;
  emailConfirmado: boolean;
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
  quantidadePedidos: number;
}

@Injectable({ providedIn: 'root' })
export class ClienteService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/clientes`;

  getAll(): Observable<ClienteDTO[]> {
    return this.http.get<ClienteDTO[]>(this.baseUrl);
  }

  getById(id: string): Observable<ClienteDTO> {
    return this.http.get<ClienteDTO>(`${this.baseUrl}/${id}`);
  }

  create(dto: Partial<ClienteDTO>) {
    return this.http.post<ClienteDTO>(this.baseUrl, dto);
  }

  update(id: string, dto: Partial<ClienteDTO>) {
    return this.http.put<ClienteDTO>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
