import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ClienteService } from '../../services/cliente.service';
import { AlertService } from '../../services/alert.service';
import { CriarCliente } from '../../models/cliente.model';

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-8 col-lg-6">
          <div class="card shadow">
            <div class="card-body p-5">
              <div class="text-center mb-4">
                <i class="bi bi-person-plus fs-1 text-primary"></i>
                <h2 class="fw-bold mt-3">Criar Conta</h2>
                <p class="text-muted">Preencha seus dados para se cadastrar</p>
              </div>

              <form (ngSubmit)="cadastrar()">
                <div class="row">
                  <div class="col-md-12 mb-3">
                    <label class="form-label">Nome Completo *</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-person"></i></span>
                      <input 
                        type="text" 
                        class="form-control" 
                        [(ngModel)]="dados.nome" 
                        name="nome"
                        placeholder="Digite seu nome completo"
                        required
                      >
                    </div>
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Email *</label>
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
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Telefone</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-telephone"></i></span>
                      <input 
                        type="tel" 
                        class="form-control" 
                        [(ngModel)]="dados.telefone" 
                        name="telefone"
                        placeholder="(11) 99999-9999"
                      >
                    </div>
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">CPF</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-card-text"></i></span>
                      <input 
                        type="text" 
                        class="form-control" 
                        [(ngModel)]="dados.cpf" 
                        name="cpf"
                        placeholder="000.000.000-00"
                      >
                    </div>
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Data de Nascimento</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-calendar"></i></span>
                      <input 
                        type="date" 
                        class="form-control" 
                        [(ngModel)]="dados.dataNascimento" 
                        name="dataNascimento"
                      >
                    </div>
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Senha *</label>
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
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Confirmar Senha *</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-lock-fill"></i></span>
                      <input 
                        [type]="mostrarConfirmarSenha ? 'text' : 'password'" 
                        class="form-control" 
                        [(ngModel)]="confirmarSenha" 
                        name="confirmarSenha"
                        placeholder="Confirme sua senha"
                        required
                      >
                      <button 
                        class="btn btn-outline-secondary" 
                        type="button"
                        (click)="mostrarConfirmarSenha = !mostrarConfirmarSenha"
                      >
                        <i class="bi" [class.bi-eye]="!mostrarConfirmarSenha" [class.bi-eye-slash]="mostrarConfirmarSenha"></i>
                      </button>
                    </div>
                  </div>
                </div>

                <div class="mb-3 form-check">
                  <input type="checkbox" class="form-check-input" id="termos" [(ngModel)]="aceitaTermos" name="termos" required>
                  <label class="form-check-label" for="termos">
                    Li e aceito os <a href="#">Termos de Uso</a> e <a href="#">Política de Privacidade</a>
                  </label>
                </div>

                <button 
                  type="submit" 
                  class="btn btn-primario w-100 btn-lg mb-3"
                  [disabled]="processando"
                >
                  <span *ngIf="!processando">
                    <i class="bi bi-person-plus me-2"></i>Criar Conta
                  </span>
                  <span *ngIf="processando">
                    <span class="spinner-border spinner-border-sm me-2"></span>
                    Criando conta...
                  </span>
                </button>

                <div *ngIf="erro" class="alert alert-danger">
                  <i class="bi bi-exclamation-triangle me-2"></i>{{ erro }}
                </div>
              </form>

              <hr class="my-4">

              <div class="text-center">
                <p class="mb-0">
                  Já tem uma conta? 
                  <a routerLink="/login" class="text-decoration-none">Entrar</a>
                </p>
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
export class CadastroComponent {
  dados: CriarCliente = {
    nome: '',
    email: '',
    telefone: '',
    cpf: '',
    dataNascimento: undefined,
    senha: ''
  };
  confirmarSenha = '';
  mostrarSenha = false;
  mostrarConfirmarSenha = false;
  aceitaTermos = false;
  processando = false;
  erro = '';

  constructor(
    private clienteService: ClienteService,
    private alertService: AlertService,
    private router: Router
  ) {}

  cadastrar(): void {
    this.erro = '';

    if (!this.dados.nome || !this.dados.email || !this.dados.senha) {
      this.alertService.warning('Atenção', 'Por favor, preencha todos os campos obrigatórios.');
      return;
    }

    if (this.dados.senha !== this.confirmarSenha) {
      this.alertService.error('Erro', 'As senhas não coincidem.');
      return;
    }

    if (!this.aceitaTermos) {
      this.alertService.warning('Atenção', 'Você deve aceitar os termos de uso.');
      return;
    }

    this.processando = true;

    this.clienteService.cadastrar(this.dados).subscribe({
      next: () => {
        this.processando = false;
        this.alertService.success('Conta criada com sucesso!', 'Faça login para continuar.');
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 1500);
      },
      error: (err) => {
        this.processando = false;
        this.erro = 'Erro ao criar conta. Tente novamente.';
        this.alertService.error('Erro ao criar conta', 'Tente novamente ou use outro email.');
      }
    });
  }
}
