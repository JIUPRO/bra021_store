import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { ApiService } from './api.service';
import { Cliente, CriarCliente, LoginCliente } from '../models/cliente.model';

@Injectable({
  providedIn: 'root'
})
export class ClienteService {
  private clienteLogadoSubject = new BehaviorSubject<Cliente | null>(null);
  clienteLogado$: Observable<Cliente | null> = this.clienteLogadoSubject.asObservable();

  constructor(private api: ApiService) {
    this.carregarClienteLogado();
  }

  private carregarClienteLogado(): void {
    const clienteSalvo = sessionStorage.getItem('clienteLogado');
    const tokenExpiracao = sessionStorage.getItem('tokenExpiracao');
    
    if (clienteSalvo && tokenExpiracao) {
      const agora = new Date().getTime();
      const expiracao = parseInt(tokenExpiracao, 10);
      
      if (agora < expiracao) {
        this.clienteLogadoSubject.next(JSON.parse(clienteSalvo));
      } else {
        // Token expirado, limpar sessão
        this.logout();
      }
    }
  }

  private salvarClienteLogado(cliente: Cliente | null): void {
    if (cliente) {
      sessionStorage.setItem('clienteLogado', JSON.stringify(cliente));
      // Token expira em 2 horas
      const expiracao = new Date().getTime() + (2 * 60 * 60 * 1000);
      sessionStorage.setItem('tokenExpiracao', expiracao.toString());
    } else {
      sessionStorage.removeItem('clienteLogado');
      sessionStorage.removeItem('tokenExpiracao');
    }
    this.clienteLogadoSubject.next(cliente);
  }

  login(dados: LoginCliente): Observable<Cliente> {
    return new Observable(observer => {
      this.api.post<Cliente>('clientes/login', dados).subscribe({
        next: (cliente) => {
          this.salvarClienteLogado(cliente);
          observer.next(cliente);
          observer.complete();
        },
        error: (err) => observer.error(err)
      });
    });
  }

  cadastrar(dados: CriarCliente): Observable<Cliente> {
    return this.api.post<Cliente>('clientes', dados);
  }

  logout(): void {
    this.salvarClienteLogado(null);
  }

  estaLogado(): boolean {
    return this.clienteLogadoSubject.value !== null;
  }

  obterClienteLogado(): Cliente | null {
    return this.clienteLogadoSubject.value;
  }

  obterPorId(id: string): Observable<Cliente> {
    return this.api.getById<Cliente>('clientes', id);
  }

  obterPorCpf(cpf: string): Observable<Cliente> {
    return this.api.get<Cliente>(`clientes/cpf/${cpf}`);
  }

  atualizar(id: string, dados: Partial<Cliente>): Observable<Cliente> {
    return this.api.put<Cliente>('clientes', id, dados);
  }
}
