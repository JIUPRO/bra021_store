import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="login-container">
      <div class="login-box">
        <div class="login-header">
          <h1 class="mb-0">
            <i class="bi bi-lock-fill me-2"></i>Backoffice
          </h1>
          <p class="text-muted small mb-0">Sistema de Administração</p>
        </div>

        <form *ngIf="!modoRecuperacao" (ngSubmit)="onSubmit()">
          <div class="form-group mb-3">
            <label for="email" class="form-label">Email</label>
            <input
              type="email"
              class="form-control"
              id="email"
              [(ngModel)]="email"
              name="email"
              required
              placeholder="seu.email@exemplo.com"
            />
          </div>

          <div class="form-group mb-3">
            <label for="senha" class="form-label">Senha</label>
            <input
              type="password"
              class="form-control"
              id="senha"
              [(ngModel)]="senha"
              name="senha"
              required
              placeholder="Digite sua senha"
            />
          </div>

          <div class="text-end mb-4">
            <button type="button" class="btn btn-link btn-sm p-0 link-recuperacao" (click)="abrirRecuperacao()">
              Esqueci minha senha
            </button>
          </div>

          <button
            type="submit"
            class="btn btn-primary w-100"
            [disabled]="carregando"
          >
            <ng-container *ngIf="!carregando">
              <i class="bi bi-box-arrow-in-right me-2"></i>Entrar
            </ng-container>
            <ng-container *ngIf="carregando">
              <span class="spinner-border spinner-border-sm me-2"></span>Entrando...
            </ng-container>
          </button>
        </form>

        <div *ngIf="modoRecuperacao">
          <div *ngIf="etapaRecuperacao === 'solicitar'; else etapaResetar">
            <div class="form-group mb-3">
              <label for="emailRecuperacao" class="form-label">Email</label>
              <input
                type="email"
                class="form-control"
                id="emailRecuperacao"
                [(ngModel)]="emailRecuperacao"
                name="emailRecuperacao"
                placeholder="seu.email@exemplo.com"
              />
            </div>

            <button
              type="button"
              class="btn btn-primary w-100 mb-3"
              [disabled]="carregando"
              (click)="solicitarRecuperacao()"
            >
              <ng-container *ngIf="!carregando">
                <i class="bi bi-envelope me-2"></i>Enviar código
              </ng-container>
              <ng-container *ngIf="carregando">
                <span class="spinner-border spinner-border-sm me-2"></span>Enviando...
              </ng-container>
            </button>
          </div>

          <ng-template #etapaResetar>
            <div class="form-group mb-3">
              <label for="codigoRecuperacao" class="form-label">Código</label>
              <input
                type="text"
                class="form-control"
                id="codigoRecuperacao"
                [(ngModel)]="codigoRecuperacao"
                name="codigoRecuperacao"
                placeholder="Digite o código recebido"
              />
            </div>

            <div class="form-group mb-3">
              <label for="novaSenha" class="form-label">Nova senha</label>
              <input
                type="password"
                class="form-control"
                id="novaSenha"
                [(ngModel)]="novaSenha"
                name="novaSenha"
                placeholder="Digite a nova senha"
              />
            </div>

            <div class="form-group mb-3">
              <label for="confirmaSenha" class="form-label">Confirmar senha</label>
              <input
                type="password"
                class="form-control"
                id="confirmaSenha"
                [(ngModel)]="confirmaSenha"
                name="confirmaSenha"
                placeholder="Confirme a nova senha"
              />
            </div>

            <button
              type="button"
              class="btn btn-primary w-100 mb-3"
              [disabled]="carregando"
              (click)="resetarSenha()"
            >
              <ng-container *ngIf="!carregando">
                <i class="bi bi-shield-lock me-2"></i>Redefinir senha
              </ng-container>
              <ng-container *ngIf="carregando">
                <span class="spinner-border spinner-border-sm me-2"></span>Redefinindo...
              </ng-container>
            </button>
          </ng-template>

          <button
            type="button"
            class="btn btn-outline-secondary w-100"
            [disabled]="carregando"
            (click)="voltarLogin()"
          >
            Voltar ao login
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      width: 100%;
      height: 100%;
      min-height: 100vh;
      background: linear-gradient(135deg, #10b981 0%, #059669 100%);
    }

    .login-box {
      background: white;
      border-radius: 12px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
      padding: 40px;
      width: 100%;
      max-width: 400px;
    }

    .login-header {
      text-align: center;
      margin-bottom: 30px;
    }

    .login-header h1 {
      color: #333;
      font-weight: 700;
      font-size: 28px;
    }

    .form-control {
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 10px 15px;
      font-size: 14px;
    }

    .form-control:focus {
      border-color: #10b981;
      box-shadow: 0 0 0 0.2rem rgba(16, 185, 129, 0.25);
    }

    .btn-primary {
      background-color: #10b981;
      border-color: #10b981;
      font-weight: 600;
      padding: 10px 15px;
      border-radius: 8px;
    }

    .btn-primary:hover {
      background-color: #059669;
      border-color: #059669;
    }

    .link-recuperacao {
      color: #059669;
      text-decoration: none;
      font-weight: 500;
    }

    .link-recuperacao:hover {
      color: #047857;
      text-decoration: underline;
    }
  `]
})
export class LoginComponent implements OnInit {
  email = '';
  senha = '';
  carregando = false;
  modoRecuperacao = false;
  etapaRecuperacao: 'solicitar' | 'resetar' = 'solicitar';
  emailRecuperacao = '';
  codigoRecuperacao = '';
  novaSenha = '';
  confirmaSenha = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    if (this.authService.estaAutenticado()) {
      this.router.navigate(['/']);
    }
  }

  onSubmit(): void {
    if (!this.email || !this.senha) {
      this.alertService.warning('Aviso', 'Preencha todos os campos');
      return;
    }

    this.carregando = true;
    this.authService.login(this.email, this.senha).subscribe({
      next: () => {
        this.alertService.success('Sucesso', 'Login realizado com sucesso!');
        this.router.navigate(['/']);
      },
      error: (err) => {
        console.error('Erro ao fazer login', err);
        this.alertService.error('Erro', 'Email ou senha incorretos');
        this.carregando = false;
      }
    });
  }

  abrirRecuperacao(): void {
    this.modoRecuperacao = true;
    this.etapaRecuperacao = 'solicitar';
    this.emailRecuperacao = this.email;
    this.codigoRecuperacao = '';
    this.novaSenha = '';
    this.confirmaSenha = '';
  }

  voltarLogin(): void {
    this.modoRecuperacao = false;
    this.etapaRecuperacao = 'solicitar';
    this.carregando = false;
  }

  solicitarRecuperacao(): void {
    if (!this.emailRecuperacao) {
      this.alertService.warning('Aviso', 'Informe o email');
      return;
    }

    this.carregando = true;
    this.authService.esqueceuSenhaUsuario(this.emailRecuperacao).subscribe({
      next: () => {
        this.alertService.success('Sucesso', 'Foi enviado o código para o seu email.');
        this.etapaRecuperacao = 'resetar';
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao solicitar recuperação de senha', err);
        this.alertService.error('Erro', err.error?.mensagem || 'Não foi possível enviar o código');
        this.carregando = false;
      }
    });
  }

  resetarSenha(): void {
    if (!this.emailRecuperacao || !this.codigoRecuperacao || !this.novaSenha || !this.confirmaSenha) {
      this.alertService.warning('Aviso', 'Preencha todos os campos');
      return;
    }

    this.carregando = true;
    this.authService.resetarSenhaUsuario(
      this.emailRecuperacao,
      this.codigoRecuperacao,
      this.novaSenha,
      this.confirmaSenha
    ).subscribe({
      next: () => {
        this.alertService.success('Sucesso', 'Senha redefinida com sucesso.');
        this.email = this.emailRecuperacao;
        this.senha = '';
        this.voltarLogin();
      },
      error: (err) => {
        console.error('Erro ao resetar senha', err);
        this.alertService.error('Erro', err.error?.mensagem || err.error?.message || 'Não foi possível redefinir a senha');
        this.carregando = false;
      }
    });
  }
}
