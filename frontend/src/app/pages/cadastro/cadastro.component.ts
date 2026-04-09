import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ClienteService } from '../../services/cliente.service';
import { AlertService } from '../../services/alert.service';
import { CepService } from '../../services/cep.service';
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
                <!-- Avisos de Campos Obrigatórios -->
                <div class="alert alert-info mb-4" *ngIf="tentouEnviar && errosValidacao.length > 0">
                  <i class="bi bi-exclamation-circle me-2"></i>
                  <strong>Preencha os campos obrigatórios:</strong>
                  <ul class="mb-0 mt-2">
                    <li *ngFor="let erro of errosValidacao">{{ erro }}</li>
                  </ul>
                </div>

                <div class="row">
                  <div class="col-md-12 mb-3">
                    <label class="form-label">Nome Completo *</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-person"></i></span>
                      <input 
                        type="text" 
                        class="form-control" 
                        [class.is-invalid]="tentouEnviar && !dados.nome"
                        [(ngModel)]="dados.nome" 
                        name="nome"
                        placeholder="Digite seu nome completo"
                        required
                      >
                    </div>
                    <small class="text-danger" *ngIf="tentouEnviar && !dados.nome">
                      <i class="bi bi-exclamation-circle me-1"></i>Nome é obrigatório
                    </small>
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
                        [class.is-invalid]="tentouEnviar && !dados.email"
                        [(ngModel)]="dados.email" 
                        name="email"
                        placeholder="seu@email.com"
                        required
                      >
                    </div>
                    <small class="text-danger" *ngIf="tentouEnviar && !dados.email">
                      <i class="bi bi-exclamation-circle me-1"></i>Email é obrigatório
                    </small>
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Telefone *</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-telephone"></i></span>
                      <input 
                        type="tel" 
                        class="form-control" 
                        [class.is-invalid]="tentouEnviar && !dados.telefone"
                        [(ngModel)]="dados.telefone" 
                        name="telefone"
                        placeholder="(11) 99999-9999"
                        (input)="aoDigitarTelefone($event)"
                        required
                      >
                    </div>
                    <small class="text-danger" *ngIf="tentouEnviar && !dados.telefone">
                      <i class="bi bi-exclamation-circle me-1"></i>Telefone é obrigatório
                    </small>
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">CPF *</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-card-text"></i></span>
                      <input 
                        type="text" 
                        class="form-control" 
                        [class.is-invalid]="tentouEnviar && !dados.cpf"
                        [(ngModel)]="dados.cpf" 
                        name="cpf"
                        placeholder="000.000.000-00"
                        (input)="aoDigitarCpf($event)"
                        required
                      >
                    </div>
                    <small class="text-danger" *ngIf="tentouEnviar && !dados.cpf">
                      <i class="bi bi-exclamation-circle me-1"></i>CPF é obrigatório
                    </small>
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

                <h5 class="mt-4 mb-3"><i class="bi bi-map me-2"></i>Endereço</h5>

                <div class="row">
                  <div class="col-md-4 mb-3">
                    <label class="form-label">CEP</label>
                    <div class="input-group">
                      <input
                        type="text"
                        class="form-control"
                        [(ngModel)]="dados.cep"
                        name="cep"
                        placeholder="00000-000"
                        (input)="aoDigitarCep($event)"
                      (blur)="buscarCep()"
                    >
                      <span class="input-group-text" *ngIf="buscandoCep">
                        <span class="spinner-border spinner-border-sm cep-spinner" role="status" aria-hidden="true"></span>
                      </span>
                    </div>
                  </div>
                  <div class="col-md-8 mb-3">
                    <label class="form-label">Logradouro</label>
                    <input
                      type="text"
                      class="form-control"
                      [(ngModel)]="dados.logradouro"
                      name="logradouro"
                      placeholder="Rua, Avenida, etc."
                    >
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Número</label>
                    <input
                      type="text"
                      class="form-control"
                      [(ngModel)]="dados.numero"
                      name="numero"
                      placeholder="123"
                    >
                  </div>
                  <div class="col-md-8 mb-3">
                    <label class="form-label">Complemento</label>
                    <input
                      type="text"
                      class="form-control"
                      [(ngModel)]="dados.complemento"
                      name="complemento"
                      placeholder="Apto, Bloco, etc."
                    >
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Bairro</label>
                    <input
                      type="text"
                      class="form-control"
                      [(ngModel)]="dados.bairro"
                      name="bairro"
                      placeholder="Bairro"
                    >
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Cidade</label>
                    <input
                      type="text"
                      class="form-control"
                      [(ngModel)]="dados.cidade"
                      name="cidade"
                      placeholder="Cidade"
                    >
                  </div>
                  <div class="col-md-2 mb-3">
                    <label class="form-label">UF</label>
                    <input
                      type="text"
                      class="form-control"
                      [(ngModel)]="dados.estado"
                      name="estado"
                      placeholder="SP"
                      maxlength="2"
                    >
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

    .cep-spinner {
      width: 0.8rem;
      height: 0.8rem;
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
    senha: '',
    cep: '',
    logradouro: '',
    numero: '',
    complemento: '',
    bairro: '',
    cidade: '',
    estado: ''
  };
  confirmarSenha = '';
  mostrarSenha = false;
  mostrarConfirmarSenha = false;
  aceitaTermos = false;
  processando = false;
  buscandoCep = false;
  erro = '';
  tentouEnviar = false;
  errosValidacao: string[] = [];
  private ultimoCepConsultado = '';

  constructor(
    private clienteService: ClienteService,
    private alertService: AlertService,
    private cepService: CepService,
    private router: Router
  ) {}

  cadastrar(): void {
    this.erro = '';
    this.tentouEnviar = true;
    this.errosValidacao = [];

    // Validação de campos obrigatórios
    if (!this.dados.nome?.trim()) {
      this.errosValidacao.push('Nome completo');
    }
    if (!this.dados.email?.trim()) {
      this.errosValidacao.push('Email');
    }
    if (!this.dados.telefone?.trim()) {
      this.errosValidacao.push('Telefone');
    }
    if (!this.dados.cpf?.trim()) {
      this.errosValidacao.push('CPF');
    }
    if (!this.dados.senha?.trim()) {
      this.errosValidacao.push('Senha');
    }
    if (!this.confirmarSenha?.trim()) {
      this.errosValidacao.push('Confirmação de Senha');
    }

    // Se há erros, mostrar e retornar
    if (this.errosValidacao.length > 0) {
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

  formatarCpf(valor: string): string {
    if (!valor) return '';
    valor = valor.replace(/\D/g, '');
    if (valor.length > 11) valor = valor.substring(0, 11);
    return valor.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
  }

  formatarTelefone(valor: string): string {
    if (!valor) return '';
    valor = valor.replace(/\D/g, '');
    if (valor.length > 11) valor = valor.substring(0, 11);
    if (valor.length <= 6) {
      return valor.replace(/(\d{0,2})(\d{0,4})/, '($1) $2').trim();
    }
    return valor.replace(/(\d{2})(\d{4,5})(\d{4})/, '($1) $2-$3');
  }

  formatarCep(valor: string): string {
    if (!valor) return '';
    valor = valor.replace(/\D/g, '');
    if (valor.length > 8) valor = valor.substring(0, 8);
    return valor.replace(/(\d{5})(\d{3})/, '$1-$2');
  }

  aoDigitarCpf(event: any): void {
    const valor = event.target.value;
    const formatado = this.formatarCpf(valor);
    this.dados.cpf = formatado;
    event.target.value = formatado;
  }

  aoDigitarTelefone(event: any): void {
    const valor = event.target.value;
    const formatado = this.formatarTelefone(valor);
    this.dados.telefone = formatado;
    event.target.value = formatado;
  }

  aoDigitarCep(event: any): void {
    const valor = event.target.value;
    const formatado = this.formatarCep(valor);
    this.dados.cep = formatado;
    event.target.value = formatado;
    const cepLimpo = formatado.replace(/\D/g, '');
    if (cepLimpo.length === 8) {
      this.buscarCep();
    }
  }

  buscarCep(): void {
    const cepLimpo = (this.dados.cep || '').replace(/\D/g, '');
    if (cepLimpo.length !== 8 || cepLimpo === this.ultimoCepConsultado) {
      return;
    }

    this.buscandoCep = true;
    this.cepService.consultarCep(cepLimpo).subscribe({
      next: (endereco) => {
        this.ultimoCepConsultado = cepLimpo;
        this.dados.logradouro = endereco.street || this.dados.logradouro;
        this.dados.bairro = endereco.neighborhood || this.dados.bairro;
        this.dados.cidade = endereco.city || this.dados.cidade;
        this.dados.estado = endereco.state || this.dados.estado;
        this.buscandoCep = false;
      },
      error: () => {
        this.buscandoCep = false;
        this.alertService.warning('CEP não encontrado', 'Não foi possível localizar esse CEP. Confira o número informado.');
      }
    });
  }
}
