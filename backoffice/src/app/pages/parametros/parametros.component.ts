import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ParametroSistemaService } from '../../services/parametro-sistema.service';
import { AlertService } from '../../services/alert.service';
import { ParametroSistema } from '../../models/parametro-sistema.model';

@Component({
  selector: 'app-parametros',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container-fluid py-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="fw-bold mb-0">
          <i class="bi bi-gear-fill me-2"></i>
          Parâmetros do Sistema
        </h1>
        <button class="btn btn-success" (click)="abrirModalNovo()">
          <i class="bi bi-plus-lg me-2"></i>
          Novo Parâmetro
        </button>
      </div>

      <div class="row">
        <div class="col-lg-8">
          <div class="card">
            <div class="card-body">
              <div *ngIf="carregando" class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                  <span class="visually-hidden">Carregando...</span>
                </div>
              </div>

              <div *ngIf="!carregando && parametros.length === 0" class="text-center py-5 text-muted">
                <i class="bi bi-inbox fs-1 d-block mb-3"></i>
                <p>Nenhum parâmetro configurado</p>
              </div>

              <div *ngIf="!carregando && parametros.length > 0">
                <div *ngFor="let parametro of parametros" class="mb-4 pb-4 border-bottom">
                  <div class="mb-2">
                    <strong class="d-block">{{ parametro.chave }}</strong>
                    <small class="text-muted" *ngIf="parametro.descricao">{{ parametro.descricao }}</small>
                  </div>

                  <div class="row align-items-end">
                    <div class="col-md-8">
                      <ng-container [ngSwitch]="parametro.chave">
                        <!-- Tipo de Endereço de Entrega -->
                        <select *ngSwitchCase="'TipoEnderecoEntrega'" 
                                class="form-select" 
                                [(ngModel)]="parametro.valor" 
                                [name]="'valor-' + parametro.id">
                          <option value="Cliente">Cliente</option>
                          <option value="Escola">Escola</option>
                          <option value="Ambos">Ambos (Cliente escolhe)</option>
                        </select>

                        <!-- URLs de Imagens do Carrossel -->
                        <div *ngSwitchCase="'CarrosselImagem1'">
                          <input type="url" 
                                 class="form-control mb-2" 
                                 [(ngModel)]="parametro.valor" 
                                 [name]="'valor-' + parametro.id"
                                 placeholder="https://exemplo.com/imagem.jpg">
                          <img *ngIf="parametro.valor" 
                               [src]="parametro.valor" 
                               class="img-thumbnail" 
                               style="max-height: 120px; max-width: 200px;"
                               onerror="this.style.display='none'">
                        </div>
                        
                        <div *ngSwitchCase="'CarrosselImagem2'">
                          <input type="url" 
                                 class="form-control mb-2" 
                                 [(ngModel)]="parametro.valor" 
                                 [name]="'valor-' + parametro.id"
                                 placeholder="https://exemplo.com/imagem.jpg">
                          <img *ngIf="parametro.valor" 
                               [src]="parametro.valor" 
                               class="img-thumbnail" 
                               style="max-height: 120px; max-width: 200px;"
                               onerror="this.style.display='none'">
                        </div>
                        
                        <div *ngSwitchCase="'CarrosselImagem3'">
                          <input type="url" 
                                 class="form-control mb-2" 
                                 [(ngModel)]="parametro.valor" 
                                 [name]="'valor-' + parametro.id"
                                 placeholder="https://exemplo.com/imagem.jpg">
                          <img *ngIf="parametro.valor" 
                               [src]="parametro.valor" 
                               class="img-thumbnail" 
                               style="max-height: 120px; max-width: 200px;"
                               onerror="this.style.display='none'">
                        </div>
                        
                        <div *ngSwitchCase="'CarrosselImagem4'">
                          <input type="url" 
                                 class="form-control mb-2" 
                                 [(ngModel)]="parametro.valor" 
                                 [name]="'valor-' + parametro.id"
                                 placeholder="https://exemplo.com/imagem.jpg">
                          <img *ngIf="parametro.valor" 
                               [src]="parametro.valor" 
                               class="img-thumbnail" 
                               style="max-height: 120px; max-width: 200px;"
                               onerror="this.style.display='none'">
                        </div>

                        <!-- Máximo de Parcelas -->
                        <input *ngSwitchCase="'MaximoParcelas'" 
                               type="number" 
                               class="form-control" 
                               [(ngModel)]="parametro.valor" 
                               [name]="'valor-' + parametro.id"
                               min="1"
                               max="12"
                               placeholder="Ex: 3">

                        <!-- Padrão - input text -->
                        <input *ngSwitchDefault 
                               type="text" 
                               class="form-control" 
                               [(ngModel)]="parametro.valor" 
                               [name]="'valor-' + parametro.id">
                      </ng-container>
                    </div>

                    <div class="col-md-4">
                      <button class="btn btn-primary w-100" 
                              (click)="salvarParametro(parametro)"
                              [disabled]="salvandoId === parametro.id">
                        <span *ngIf="salvandoId !== parametro.id">
                          <i class="bi bi-check-lg me-2"></i>Salvar
                        </span>
                        <span *ngIf="salvandoId === parametro.id">
                          <span class="spinner-border spinner-border-sm me-2"></span>
                          Salvando...
                        </span>
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-lg-4">
          <div class="card bg-light">
            <div class="card-body">
              <h5 class="card-title">
                <i class="bi bi-info-circle me-2"></i>
                Sobre os Parâmetros
              </h5>
              <p class="card-text small">
                <strong>Tipo de Endereço de Entrega:</strong><br>
                Define como o endereço de entrega será tratado no checkout:
              </p>
              <ul class="small mb-3">
                <li><strong>Cliente:</strong> Usa sempre o endereço cadastrado do cliente</li>
                <li><strong>Escola:</strong> Usa sempre o endereço da escola selecionada</li>
                <li><strong>Ambos:</strong> Permite que o cliente escolha entre seu endereço e o da escola</li>
              </ul>
              
              <hr>
              
              <p class="card-text small">
                <strong>Carrossel da Home:</strong><br>
                Configure até 4 imagens para exibir no carrossel da página inicial da loja. 
                Cole a URL completa da imagem hospedada (ex: https://i.imgur.com/imagem.jpg).
              </p>
              <p class="small text-muted mb-3">
                <i class="bi bi-lightbulb me-1"></i>
                <strong>Dica:</strong> Use imagens com proporção 3:2 (ex: 1200x800px) para melhor resultado.
              </p>

              <hr>

              <p class="card-text small">
                <strong>Máximo de Parcelas:</strong><br>
                Define quantas vezes o cliente pode parcelar uma compra no cartão de crédito sem juros. 
                Valor padrão: 3x.
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal Novo Parâmetro -->
      <div class="modal fade" [class.show]="mostrarModal" [style.display]="mostrarModal ? 'block' : 'none'" 
           tabindex="-1" (click)="fecharModalSeBackdrop($event)">
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Novo Parâmetro</h5>
              <button type="button" class="btn-close" (click)="fecharModal()"></button>
            </div>
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label">Chave <span class="text-danger">*</span></label>
                <input type="text" class="form-control" [(ngModel)]="novoParametro.chave" 
                       placeholder="Ex: MaximoParcelas">
                <small class="text-muted">Nome único do parâmetro (sem espaços)</small>
              </div>
              <div class="mb-3">
                <label class="form-label">Valor <span class="text-danger">*</span></label>
                <input type="text" class="form-control" [(ngModel)]="novoParametro.valor" 
                       placeholder="Ex: 3">
              </div>
              <div class="mb-3">
                <label class="form-label">Descrição</label>
                <textarea class="form-control" rows="2" [(ngModel)]="novoParametro.descricao"
                          placeholder="Descrição do que este parâmetro faz"></textarea>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" (click)="fecharModal()">Cancelar</button>
              <button type="button" class="btn btn-success" (click)="criarParametro()"
                      [disabled]="!novoParametro.chave || !novoParametro.valor || criando">
                <span *ngIf="!criando">
                  <i class="bi bi-check-lg me-2"></i>Criar
                </span>
                <span *ngIf="criando">
                  <span class="spinner-border spinner-border-sm me-2"></span>
                  Criando...
                </span>
              </button>
            </div>
          </div>
        </div>
      </div>
      <div class="modal-backdrop fade" [class.show]="mostrarModal" *ngIf="mostrarModal"></div>
    </div>
  `,
  styles: []
})
export class ParametrosComponent implements OnInit {
  parametros: ParametroSistema[] = [];
  carregando = false;
  salvandoId: string | null = null;
  mostrarModal = false;
  criando = false;
  novoParametro = {
    chave: '',
    valor: '',
    descricao: ''
  };

  constructor(
    private parametroService: ParametroSistemaService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.carregarParametros();
  }

  carregarParametros(): void {
    this.carregando = true;
    this.parametroService.obterTodos().subscribe({
      next: (parametros) => {
        this.parametros = parametros;
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao carregar parâmetros', err);
        this.alertService.error('Erro', 'Erro ao carregar parâmetros');
        this.carregando = false;
      }
    });
  }

  salvarParametro(parametro: ParametroSistema): void {
    this.salvandoId = parametro.id;
    // Garantir que o valor é sempre enviado como string
    const valor = String(parametro.valor);
    this.parametroService.atualizar(parametro.id, { valor }).subscribe({
      next: () => {
        this.salvandoId = null;
        this.alertService.success('Sucesso', 'Parâmetro atualizado com sucesso!');
        this.carregarParametros();
      },
      error: (err) => {
        console.error('Erro ao salvar parâmetro', err);
        const mensagem = err.error?.mensagem || err.message || 'Erro desconhecido';
        this.alertService.error('Erro', `Erro ao salvar parâmetro: ${mensagem}`);
        this.salvandoId = null;
      }
    });
  }

  abrirModalNovo(): void {
    this.novoParametro = { chave: '', valor: '', descricao: '' };
    this.mostrarModal = true;
  }

  fecharModal(): void {
    this.mostrarModal = false;
    this.novoParametro = { chave: '', valor: '', descricao: '' };
  }

  fecharModalSeBackdrop(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.fecharModal();
    }
  }

  criarParametro(): void {
    this.criando = true;
    // Garantir que os valores são sempre strings
    const dados = {
      chave: String(this.novoParametro.chave),
      valor: String(this.novoParametro.valor),
      descricao: this.novoParametro.descricao ? String(this.novoParametro.descricao) : undefined
    };
    this.parametroService.criar(dados).subscribe({
      next: () => {
        this.criando = false;
        this.fecharModal();
        this.alertService.success('Sucesso', 'Parâmetro criado com sucesso!');
        this.carregarParametros();
      },
      error: (err: any) => {
        console.error('Erro ao criar parâmetro', err);
        const mensagem = err.error?.mensagem || err.error?.message || 'Erro desconhecido';
        this.alertService.error('Erro', `Erro ao criar parâmetro: ${mensagem}`);
        this.criando = false;
      }
    });
  }
}
