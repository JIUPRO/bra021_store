import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ClienteService } from '../../services/cliente.service';
import { AlertService } from '../../services/alert.service';
import { Cliente } from '../../models/cliente.model';

@Component({
  selector: 'app-editar-cadastro',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-8 col-lg-6">
          <div class="card shadow">
            <div class="card-body p-5">
              <div class="text-center mb-4">
                <i class="bi bi-person-gear fs-1 text-primary"></i>
                <h2 class="fw-bold mt-3">Editar Cadastro</h2>
                <p class="text-muted">Atualize seus dados pessoais</p>
              </div>

              <div *ngIf="carregando" class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                  <span class="visually-hidden">Carregando...</span>
                </div>
                <p class="mt-3 text-muted">Carregando seus dados...</p>
              </div>

              <form (ngSubmit)="atualizar()" *ngIf="!carregando">
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
                        disabled
                      >
                    </div>
                    <small class="text-muted">Email não pode ser alterado</small>
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

                <h5 class="mt-4 mb-3"><i class="bi bi-map me-2"></i>Endereço</h5>

                <div class="row">
                  <div class="col-md-3 mb-3">
                    <label class="form-label">CEP</label>
                    <input 
                      type="text" 
                      class="form-control" 
                      [(ngModel)]="dados.cep" 
                      name="cep"
                      placeholder="00000-000"
                      (input)="aoDigitarCep($event)"
                      (blur)="buscarCep()"
                    >
                  </div>
                  <div class="col-md-9 mb-3">
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
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Cidade</label>
                    <input 
                      type="text" 
                      class="form-control" 
                      [(ngModel)]="dados.cidade" 
                      name="cidade"
                      placeholder="Cidade"
                    >
                  </div>
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Estado</label>
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

                <div class="d-grid gap-2 mt-4">
                  <button 
                    type="submit" 
                    class="btn btn-primario btn-lg"
                    [disabled]="processando"
                  >
                    <span *ngIf="!processando">
                      <i class="bi bi-check-circle me-2"></i>Salvar Alterações
                    </span>
                    <span *ngIf="processando">
                      <i class="spinner-border spinner-border-sm me-2"></i>Salvando...
                    </span>
                  </button>
                  <a routerLink="/meus-pedidos" class="btn btn-outline-secondary btn-lg">
                    <i class="bi bi-arrow-left me-2"></i>Voltar
                  </a>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .btn-primario {
      background: var(--cor-primaria);
      color: white;
      border: none;
      font-weight: 600;
      transition: all 0.3s ease;
    }
    
    .btn-primario:hover:not(:disabled) {
      background: var(--cor-primaria-claro);
      transform: translateY(-2px);
      box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
    }
    
    .btn-primario:disabled {
      background: var(--cor-secundaria);
      cursor: not-allowed;
    }
    
    .card {
      border: none;
      border-radius: 16px;
      margin-top: 20px;
    }
    
    .text-danger {
      color: var(--cor-perigo) !important;
    }
    
    .is-invalid {
      border-color: var(--cor-perigo) !important;
    }
    
    h5 {
      font-weight: 600;
      color: var(--cor-escura);
    }
    
    .spinner-border {
      width: 50px;
      height: 50px;
    }
  `]
})
export class EditarCadastroComponent implements OnInit {
  dados: any = {
    id: '',
    nome: '',
    email: '',
    telefone: '',
    cpf: '',
    dataNascimento: undefined,
    cep: '',
    logradouro: '',
    numero: '',
    complemento: '',
    bairro: '',
    cidade: '',
    estado: ''
  };

  carregando = true;
  processando = false;
  tentouEnviar = false;
  errosValidacao: string[] = [];

  constructor(
    private clienteService: ClienteService,
    private alertService: AlertService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.carregarDados();
  }

  carregarDados(): void {
    const cliente = this.clienteService.obterClienteLogado();
    
    if (!cliente) {
      this.alertService.warning('Atenção', 'Você não está logado. Faça login para continuar.');
      this.router.navigate(['/login']);
      return;
    }

    this.dados = {
      id: cliente.id,
      nome: cliente.nome,
      email: cliente.email,
      telefone: cliente.telefone || '',
      cpf: cliente.cpf || '',
      dataNascimento: cliente.dataNascimento ? new Date(cliente.dataNascimento) : undefined,
      cep: cliente.cep || '',
      logradouro: cliente.logradouro || '',
      numero: cliente.numero || '',
      complemento: cliente.complemento || '',
      bairro: cliente.bairro || '',
      cidade: cliente.cidade || '',
      estado: cliente.estado || ''
    };

    this.carregando = false;
  }

  atualizar(): void {
    this.tentouEnviar = true;
    this.errosValidacao = [];

    // Validação de campos obrigatórios
    if (!this.dados.nome?.trim()) {
      this.errosValidacao.push('Nome completo');
    }
    if (!this.dados.telefone?.trim()) {
      this.errosValidacao.push('Telefone');
    }
    if (!this.dados.cpf?.trim()) {
      this.errosValidacao.push('CPF');
    }

    if (this.errosValidacao.length > 0) {
      this.alertService.warning('Atenção', 'Por favor, preencha todos os campos obrigatórios.');
      return;
    }

    this.processando = true;

    // Preparar DTO para atualização
    const atualizarClienteDTO = {
      id: this.dados.id,
      nome: this.dados.nome,
      email: this.dados.email,
      telefone: this.dados.telefone,
      cpf: this.dados.cpf,
      dataNascimento: this.dados.dataNascimento,
      cep: this.dados.cep,
      logradouro: this.dados.logradouro,
      numero: this.dados.numero,
      complemento: this.dados.complemento,
      bairro: this.dados.bairro,
      cidade: this.dados.cidade,
      estado: this.dados.estado
    };

    this.clienteService.atualizar(this.dados.id, atualizarClienteDTO).subscribe({
      next: (clienteAtualizado) => {
        this.processando = false;
        this.alertService.success('Sucesso!', 'Seus dados foram atualizados com sucesso.');
        
        // Atualizar o cliente logado no serviço
        this.clienteService.atualizarClienteLogado(clienteAtualizado);
        
        setTimeout(() => {
          this.router.navigate(['/meus-pedidos']);
        }, 1500);
      },
      error: (err) => {
        this.processando = false;
        this.alertService.error('Erro', 'Não foi possível atualizar seus dados. Tente novamente.');
        console.error('Erro ao atualizar cliente', err);
      }
    });
  }

  buscarCep(): void {
    // Aqui você pode integrar com uma API de CEP se desejar
    console.log('Buscar CEP:', this.dados.cep);
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
  }
}
