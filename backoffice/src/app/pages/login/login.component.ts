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

        <form (ngSubmit)="onSubmit()">
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

          <div class="form-group mb-4">
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

          <button
            type="submit"
            class="btn btn-primary w-100 mb-3"
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

    .modal {
      background: rgba(0, 0, 0, 0.5);
    }

    .modal.show {
      background: rgba(0, 0, 0, 0.5);
    }

    .modal-content {
      border-radius: 12px;
      border: none;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
    }

    .modal-header {
      background: #f8f9fa;
      border-bottom: 1px solid #ddd;
      border-radius: 12px 12px 0 0;
    }
  `]
})
export class LoginComponent implements OnInit {
  email = '';
  senha = '';
  carregando = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    // Se já está autenticado, redireciona para dashboard
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
}
