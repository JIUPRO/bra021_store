import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface Usuario {
  id: string;
  email: string;
  nome: string;
  ativo: boolean;
}

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface LoginResponse {
  token: string;
  usuario: Usuario;
}

export interface CadastroRequest {
  email: string;
  nome: string;
  senha: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private usuarioSubject = new BehaviorSubject<Usuario | null>(this.obterUsuarioArmazenado());
  public usuario$ = this.usuarioSubject.asObservable();

  constructor(private http: HttpClient) {
    this.verificarToken();
  }

  private verificarToken(): void {
    const token = sessionStorage.getItem('token');
    if (token) {
      // Se tiver token, considera autenticado
      const usuarioArmazenado = this.obterUsuarioArmazenado();
      if (usuarioArmazenado) {
        this.usuarioSubject.next(usuarioArmazenado);
      }
    }
  }

  login(email: string, senha: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, senha }).pipe(
      tap(response => {
        const token = (response as any).token ?? (response as any).Token;
        const usuario = (response as any).usuario ?? (response as any).Usuario;
        if (token) {
          sessionStorage.setItem('token', token);
        }
        if (usuario) {
          sessionStorage.setItem('usuario', JSON.stringify(usuario));
          this.usuarioSubject.next(usuario);
        }
      })
    );
  }

  cadastro(email: string, nome: string, senha: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/registro`, { email, nome, senha }).pipe(
      tap(response => {
        const token = (response as any).token ?? (response as any).Token;
        const usuario = (response as any).usuario ?? (response as any).Usuario;
        if (token) {
          sessionStorage.setItem('token', token);
        }
        if (usuario) {
          sessionStorage.setItem('usuario', JSON.stringify(usuario));
          this.usuarioSubject.next(usuario);
        }
      })
    );
  }

  logout(): void {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('usuario');
    this.usuarioSubject.next(null);
  }

  obterToken(): string | null {
    return sessionStorage.getItem('token');
  }

  obterUsuarioArmazenado(): Usuario | null {
    const usuarioStr = sessionStorage.getItem('usuario');
    return usuarioStr ? JSON.parse(usuarioStr) : null;
  }

  estaAutenticado(): boolean {
    return !!this.obterToken();
  }

  obterUsuarioAtual(): Usuario | null {
    return this.usuarioSubject.getValue();
  }

  listarUsuarios(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(`${this.apiUrl}/usuarios`);
  }

  atualizarUsuario(id: string, usuario: Partial<Usuario>): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.apiUrl}/usuarios/${id}`, usuario);
  }

  deletarUsuario(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/usuarios/${id}`);
  }

  criarUsuario(email: string, nome: string, senha: string): Observable<Usuario> {
    return this.http.post<Usuario>(`${this.apiUrl}/usuarios`, { email, nome, senha });
  }
}
