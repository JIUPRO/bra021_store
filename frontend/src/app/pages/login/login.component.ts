import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ClienteService } from '../../services/cliente.service';
import { AlertService } from '../../services/alert.service';
import { LoginCliente } from '../../models/cliente.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-6 col-lg-5">
          <div class="card shadow">
            <div class="card-body p-5">
              <div class="text-center mb-4">
                <i class="bi bi-person-circle fs-1 text-primary"></i>
                <h2 class="fw-bold mt-3">Entrar</h2>
                <p class="text-muted">Acesse sua conta para continuar</p>
              </div>

              <form (ngSubmit)="login()">
                <div class="mb-3">
                  <label class="form-label">Email</label>
                  <div class="input-group">
                    <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                    <input 
                      type="email" 
                      class="form-control" 
                      [(ngModel)]="dados.email" 
                      name="email"
                      placeholder="seu@email.com"
                      required
                    >
                  </div>
                </div>

                <div class="mb-3">
                  <label class="form-label">Senha</label>
                  <div class="input-group">
                    <span class="input-group-text"><i class="bi bi-lock"></i></span>
                    <input 
                      [type]="mostrarSenha ? 'text' : 'password'" 
                      class="form-control" 
                      [(ngModel)]="dados.senha" 
                      name="senha"
                      placeholder="Digite sua senha"
                      required
                    >
                    <button 
                      class="btn btn-outline-secondary" 
                      type="button"
                      (click)="mostrarSenha = !mostrarSenha"
                    >
                      <i class="bi" [class.bi-eye]="!mostrarSenha" [class.bi-eye-slash]="mostrarSenha"></i>
                    </button>
                  </div>
                </div>

                <div class="mb-3 form-check">
                  <input type="checkbox" class="form-check-input" id="lembrar">
                  <label class="form-check-label" for="lembrar">Lembrar-me</label>
                </div>

                <button 
                  type="submit" 
                  class="btn btn-primario w-100 btn-lg mb-3"
                  [disabled]="processando"
                >
                  <span *ngIf="!processando">
                    <i class="bi bi-box-arrow-in-right me-2"></i>Entrar
                  </span>
                  <span *ngIf="processando">
                    <span class="spinner-border spinner-border-sm me-2"></span>
                    Entrando...
                  </span>
                </button>

                <div *ngIf="erro" class="alert alert-danger">
                  <i class="bi bi-exclamation-triangle me-2"></i>{{ erro }}
                </div>
              </form>

              <hr class="my-4">

              <div class="text-center">
                <p class="mb-2">
                  Não tem uma conta? 
                  <a routerLink="/cadastro" class="text-decoration-none">Cadastre-se</a>
                </p>
                <a href="#" class="text-decoration-none small">Esqueceu sua senha?</a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .btn-primario {
      background: linear-gradient(135deg, var(--cor-primaria), var(--cor-primaria-claro));
      border: none;
      border-radius: 8px;
      padding: 12px 24px;
      font-weight: 600;
      transition: all 0.3s ease;
      color: var(--cor-escura);
    }
    
    .btn-primario:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 5px 15px rgba(47,106,73,0.18);
      color: var(--cor-escura);
    }
    
    .btn-primario:disabled {
      background: var(--cor-secundaria);
      cursor: not-allowed;
    }
    
    .card {
      border: none;
      border-radius: 16px;
    }
  `]
})
export class LoginComponent {
  dados: LoginCliente = {
    email: '',
    senha: ''
  };
  mostrarSenha = false;
  processando = false;
  erro = '';

  constructor(
    private clienteService: ClienteService,
    private alertService: AlertService,
    private router: Router
  ) {}

  login(): void {
    if (!this.dados.email || !this.dados.senha) {
      this.alertService.warning('Atenção', 'Por favor, preencha todos os campos.');
      return;
    }

    this.processando = true;
    this.erro = '';

    this.clienteService.login(this.dados).subscribe({
      next: () => {
        this.processando = false;
        this.alertService.success('Bem-vindo!', 'Login realizado com sucesso!');
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 1000);
      },
      error: (err) => {
        this.processando = false;
        this.erro = 'Email ou senha inválidos.';
        this.alertService.error('Erro de autenticação', 'Email ou senha inválidos.');
      }
    });
  }
}
