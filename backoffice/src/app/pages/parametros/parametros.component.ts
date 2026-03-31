import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ParametroSistemaService } from '../../services/parametro-sistema.service';
import { AlertService } from '../../services/alert.service';
import { ParametroSistema } from '../../models/parametro-sistema.model';

interface SecaoParametro {
  id: string;
  titulo: string;
  icone: string;
  descricao: string;
}

@Component({
  selector: 'app-parametros',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container-fluid py-4 pagina-parametros">
      <div class="topbar card border-0 shadow-sm mb-4">
        <div class="card-body d-flex flex-column flex-lg-row gap-3 justify-content-between align-items-lg-center">
          <div>
            <h1 class="fw-bold mb-1">
              <i class="bi bi-sliders me-2"></i>Configurações do Sistema
            </h1>
            <p class="text-muted mb-0">Parâmetros agrupados por seção, com edição em lote.</p>
          </div>

          <div class="d-flex flex-wrap gap-2 align-items-center">
            <span class="badge badge-pendente px-3 py-2" *ngIf="pendenciasCount > 0">
              {{ pendenciasCount }} alteração(ões) pendente(s)
            </span>
            <button class="btn btn-outline-secondary" (click)="abrirModalNovo()">
              <i class="bi bi-plus-lg me-2"></i>Novo Parâmetro
            </button>
            <button class="btn btn-outline-secondary" (click)="descartarAlteracoes()" [disabled]="pendenciasCount === 0 || salvandoTudo">
              <i class="bi bi-arrow-counterclockwise me-2"></i>Descartar
            </button>
            <button class="btn btn-success" (click)="salvarTudo()" [disabled]="pendenciasCount === 0 || salvandoTudo">
              <span *ngIf="!salvandoTudo"><i class="bi bi-check-lg me-2"></i>Salvar Alterações</span>
              <span *ngIf="salvandoTudo"><span class="spinner-border spinner-border-sm me-2"></span>Salvando...</span>
            </button>
          </div>
        </div>
      </div>

      <div *ngIf="carregando" class="text-center py-5">
        <div class="spinner-border text-success" role="status">
          <span class="visually-hidden">Carregando...</span>
        </div>
      </div>

      <div *ngIf="!carregando && parametros.length === 0" class="card border-0 shadow-sm">
        <div class="card-body text-center py-5 text-muted">
          <i class="bi bi-inbox fs-1 d-block mb-3"></i>
          <p class="mb-0">Nenhum parâmetro configurado</p>
        </div>
      </div>

      <div *ngIf="!carregando && parametros.length > 0" class="row g-4">
        <div class="col-xl-3">
          <div class="card border-0 shadow-sm sticky-panel">
            <div class="card-body">
              <label class="form-label small text-uppercase text-muted fw-semibold">Buscar configuração</label>
              <div class="input-group mb-3">
                <span class="input-group-text bg-white"><i class="bi bi-search"></i></span>
                <input type="text" class="form-control" [(ngModel)]="termoBusca" placeholder="Nome, chave ou descrição">
              </div>

              <div class="nav flex-column gap-2">
                <button
                  *ngFor="let secao of secoesVisiveis"
                  type="button"
                  class="btn btn-secao text-start"
                  [class.active]="secaoAtiva === secao.id"
                  (click)="selecionarSecao(secao.id)">
                  <div class="d-flex justify-content-between align-items-center">
                    <span><i class="bi me-2" [class]="secao.icone"></i>{{ secao.titulo }}</span>
                    <span class="badge badge-secao">{{ contarParametrosSecao(secao.id) }}</span>
                  </div>
                  <small class="text-muted d-block mt-1">{{ secao.descricao }}</small>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="col-xl-6">
          <div class="card border-0 shadow-sm">
            <div class="card-header bg-white border-0 pb-0">
              <div *ngIf="secaoAtual as secao">
                <h4 class="fw-bold mb-1">
                  <i class="bi me-2" [class]="secao.icone"></i>{{ secao.titulo }}
                </h4>
                <p class="text-muted mb-0">{{ secao.descricao }}</p>
              </div>
            </div>
            <div class="card-body pt-4">
              <div *ngIf="parametrosDaSecaoAtual.length === 0" class="text-center py-5 text-muted">
                <i class="bi bi-funnel fs-2 d-block mb-3"></i>
                Nenhum parâmetro encontrado nesta seção com o filtro atual.
              </div>

              <div *ngFor="let parametro of parametrosDaSecaoAtual" class="param-card" [class.changed]="isChanged(parametro)">
                <div class="d-flex justify-content-between align-items-start gap-3 mb-3">
                  <div>
                    <label class="form-label fw-semibold mb-1">{{ getLabel(parametro) }}</label>
                    <small class="text-muted d-block">{{ parametro.chave }}</small>
                    <small class="text-muted d-block mt-1" *ngIf="parametro.descricao">{{ parametro.descricao }}</small>
                  </div>
                  <span *ngIf="isChanged(parametro)" class="badge text-bg-warning">Alterado</span>
                </div>

                <ng-container [ngSwitch]="getCampoTipo(parametro)">
                  <div *ngSwitchCase="'select-tipo-entrega'">
                    <select class="form-select" [(ngModel)]="parametro.valor" [name]="'valor-' + parametro.id">
                      <option value="Cliente">Cliente</option>
                      <option value="Escola">Escola</option>
                      <option value="Ambos">Ambos (cliente escolhe)</option>
                    </select>
                  </div>

                  <div *ngSwitchCase="'select-frete-provider'">
                    <select class="form-select" [(ngModel)]="parametro.valor" [name]="'valor-' + parametro.id">
                      <option value="Fixo">Fixo</option>
                      <option value="MelhorEnvio">Melhor Envio</option>
                    </select>
                  </div>

                  <div *ngSwitchCase="'toggle'">
                    <div class="toggle-group">
                      <button
                        type="button"
                        class="btn"
                        [class.btn-success]="parametro.valor === 'true'"
                        [class.btn-outline-success]="parametro.valor !== 'true'"
                        (click)="parametro.valor = 'true'">
                        Ligado
                      </button>
                      <button
                        type="button"
                        class="btn"
                        [class.btn-secondary]="parametro.valor === 'false'"
                        [class.btn-outline-secondary]="parametro.valor !== 'false'"
                        (click)="parametro.valor = 'false'">
                        Desligado
                      </button>
                    </div>
                  </div>

                  <div *ngSwitchCase="'number'">
                    <input type="number" class="form-control" [(ngModel)]="parametro.valor" [name]="'valor-' + parametro.id"
                           min="0" step="1" [placeholder]="getPlaceholder(parametro)">
                  </div>

                  <div *ngSwitchCase="'cep'">
                    <input type="text" class="form-control" [(ngModel)]="parametro.valor" [name]="'valor-' + parametro.id"
                           maxlength="9" [placeholder]="getPlaceholder(parametro)">
                  </div>

                  <div *ngSwitchCase="'url'">
                    <input type="url" class="form-control mb-3" [(ngModel)]="parametro.valor" [name]="'valor-' + parametro.id"
                           [placeholder]="getPlaceholder(parametro)">
                    <img *ngIf="parametro.valor" [src]="parametro.valor" class="img-preview" alt="Preview" onerror="this.style.display='none'">
                  </div>

                  <div *ngSwitchDefault>
                    <input type="text" class="form-control" [(ngModel)]="parametro.valor" [name]="'valor-' + parametro.id"
                           [placeholder]="getPlaceholder(parametro)">
                  </div>
                </ng-container>
              </div>
            </div>
          </div>
        </div>

        <div class="col-xl-3">
          <div class="card border-0 shadow-sm sticky-panel">
            <div class="card-body">
              <h5 class="fw-bold mb-3"><i class="bi bi-lightning-charge me-2"></i>Painel de Edição</h5>

              <div class="status-box mb-3">
                <small class="text-muted d-block mb-1">Seção atual</small>
                <strong>{{ secaoAtual?.titulo || 'Nenhuma' }}</strong>
              </div>

              <div class="status-box mb-3">
                <small class="text-muted d-block mb-1">Alterações pendentes</small>
                <strong>{{ pendenciasCount }}</strong>
              </div>

              <div class="status-box mb-4">
                <small class="text-muted d-block mb-1">Busca</small>
                <strong>{{ termoBusca ? 'Filtro ativo' : 'Sem filtro' }}</strong>
              </div>

              <div *ngIf="pendencias.length > 0; else semPendencias" class="mb-4">
                <label class="form-label small text-uppercase text-muted fw-semibold">Campos alterados</label>
                <div class="lista-pendencias">
                  <div class="pendencia-item" *ngFor="let parametro of pendencias">
                    <strong>{{ getLabel(parametro) }}</strong>
                    <small class="text-muted d-block">{{ parametro.chave }}</small>
                    <small class="text-muted d-block">{{ parametro.secao || 'Outros' }}</small>
                  </div>
                </div>
              </div>

              <ng-template #semPendencias>
                <div class="alert alert-light border">
                  <small class="mb-0 d-block">Nenhuma alteração pendente no momento.</small>
                </div>
              </ng-template>

              <div class="d-grid gap-2">
                <button class="btn btn-success" (click)="salvarTudo()" [disabled]="pendenciasCount === 0 || salvandoTudo">
                  <span *ngIf="!salvandoTudo"><i class="bi bi-check-lg me-2"></i>Salvar Alterações</span>
                  <span *ngIf="salvandoTudo"><span class="spinner-border spinner-border-sm me-2"></span>Salvando...</span>
                </button>
                <button class="btn btn-outline-secondary" (click)="descartarAlteracoes()" [disabled]="pendenciasCount === 0 || salvandoTudo">
                  <i class="bi bi-arrow-counterclockwise me-2"></i>Descartar
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="modal fade" [class.show]="mostrarModal" [style.display]="mostrarModal ? 'block' : 'none'"
           tabindex="-1" (click)="fecharModalSeBackdrop($event)">
        <div class="modal-dialog modal-lg">
          <div class="modal-content border-0 shadow">
            <div class="modal-header">
              <h5 class="modal-title">Novo Parâmetro</h5>
              <button type="button" class="btn-close" (click)="fecharModal()"></button>
            </div>
            <div class="modal-body">
              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label">Chave <span class="text-danger">*</span></label>
                  <input type="text" class="form-control" [(ngModel)]="novoParametro.chave" placeholder="Ex: MaximoParcelas">
                </div>
                <div class="col-md-6">
                  <label class="form-label">Valor <span class="text-danger">*</span></label>
                  <input type="text" class="form-control" [(ngModel)]="novoParametro.valor" placeholder="Ex: 3">
                </div>
                <div class="col-md-7">
                  <label class="form-label">Seção</label>
                  <select class="form-select" [(ngModel)]="novoParametro.secao" [disabled]="usarNovaSecao">
                    <option *ngFor="let secao of secoesExistentes" [value]="secao">{{ secao }}</option>
                  </select>
                </div>
                <div class="col-md-5 d-flex align-items-end">
                  <button type="button" class="btn btn-outline-success w-100" (click)="usarNovaSecao = !usarNovaSecao">
                    <i class="bi bi-folder-plus me-2"></i>{{ usarNovaSecao ? 'Usar seção existente' : 'Criar nova seção' }}
                  </button>
                </div>
                <div class="col-12" *ngIf="usarNovaSecao">
                  <label class="form-label">Nova seção</label>
                  <input type="text" class="form-control" [(ngModel)]="novoParametro.novaSecao" placeholder="Ex: Fiscal">
                </div>
                <div class="col-12">
                  <label class="form-label">Descrição</label>
                  <textarea class="form-control" rows="3" [(ngModel)]="novoParametro.descricao" placeholder="Descrição do parâmetro"></textarea>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-outline-secondary" (click)="fecharModal()">Cancelar</button>
              <button type="button" class="btn btn-success" (click)="criarParametro()"
                      [disabled]="!novoParametro.chave || !novoParametro.valor || (usarNovaSecao && !novoParametro.novaSecao.trim()) || criando">
                <span *ngIf="!criando"><i class="bi bi-check-lg me-2"></i>Criar</span>
                <span *ngIf="criando"><span class="spinner-border spinner-border-sm me-2"></span>Criando...</span>
              </button>
            </div>
          </div>
        </div>
      </div>
      <div class="modal-backdrop fade" [class.show]="mostrarModal" *ngIf="mostrarModal"></div>
    </div>
  `,
  styles: [`
    .pagina-parametros {
      --painel-bg: linear-gradient(180deg, #f4fbf7 0%, #eaf6ef 100%);
    }

    .topbar {
      background: var(--painel-bg);
    }

    .badge-pendente {
      background: #ecfdf3;
      color: #166534;
      border: 1px solid #86efac;
      border-radius: 999px;
    }

    .sticky-panel {
      position: sticky;
      top: 88px;
    }

    .btn-secao {
      border: 1px solid #d9e7df;
      border-radius: 14px;
      background: #fff;
      padding: 14px 16px;
    }

    .btn-secao:hover {
      border-color: #a7d7bb;
      background: #f5fbf7;
    }

    .btn-secao.active {
      border-color: #198754;
      background: linear-gradient(135deg, #198754 0%, #22c55e 100%);
      color: #fff;
    }

    .btn-secao.active .text-muted {
      color: rgba(255, 255, 255, 0.82) !important;
    }

    .badge-secao {
      background: #dcfce7;
      color: #166534;
      border: 1px solid #86efac;
      border-radius: 999px;
      min-width: 36px;
    }

    .btn-secao.active .badge-secao {
      background: rgba(255, 255, 255, 0.18);
      color: #fff;
      border-color: rgba(255, 255, 255, 0.32);
    }

    .param-card {
      border: 1px solid #e5e7eb;
      border-radius: 18px;
      padding: 20px;
      background: #fff;
      margin-bottom: 18px;
      transition: border-color .2s ease, box-shadow .2s ease;
    }

    .param-card.changed {
      border-color: #f59e0b;
      box-shadow: 0 0 0 3px rgba(245, 158, 11, 0.12);
    }

    .toggle-group {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .img-preview {
      width: 100%;
      max-height: 220px;
      object-fit: cover;
      border-radius: 14px;
      border: 1px solid #e5e7eb;
    }

    .status-box {
      padding: 14px 16px;
      border-radius: 14px;
      background: #f5fbf7;
      border: 1px solid #d9e7df;
    }

    .lista-pendencias {
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-height: 260px;
      overflow: auto;
    }

    .pendencia-item {
      padding: 12px 14px;
      border-radius: 12px;
      background: #fff7ed;
      border: 1px solid #fdba74;
    }

    @media (max-width: 1199px) {
      .sticky-panel {
        position: static;
      }
    }
  `]
})
export class ParametrosComponent implements OnInit {
  parametros: ParametroSistema[] = [];
  carregando = false;
  salvandoTudo = false;
  mostrarModal = false;
  criando = false;
  termoBusca = '';
  secaoAtiva = 'Entrega';
  valoresOriginais: Record<string, string> = {};
  secoesExistentes: string[] = [];
  usarNovaSecao = false;

  novoParametro = {
    chave: '',
    valor: '',
    descricao: '',
    secao: 'Outros',
    novaSecao: ''
  };

  constructor(
    private parametroService: ParametroSistemaService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.carregarParametros();
  }

  get secoes(): SecaoParametro[] {
    return this.secoesExistentes.map(secao => ({
      id: secao,
      titulo: secao,
      icone: this.getIconeSecao(secao),
      descricao: this.getDescricaoSecao(secao)
    }));
  }

  get secaoAtual(): SecaoParametro | undefined {
    return this.secoes.find(secao => secao.id === this.secaoAtiva);
  }

  get secoesVisiveis(): SecaoParametro[] {
    return this.secoes.filter(secao => this.contarParametrosSecao(secao.id) > 0);
  }

  get parametrosFiltrados(): ParametroSistema[] {
    const termo = this.termoBusca.trim().toLowerCase();
    if (!termo) {
      return this.parametros;
    }

    return this.parametros.filter(parametro =>
      parametro.chave.toLowerCase().includes(termo) ||
      (parametro.descricao || '').toLowerCase().includes(termo) ||
      this.getLabel(parametro).toLowerCase().includes(termo) ||
      (parametro.secao || 'Outros').toLowerCase().includes(termo)
    );
  }

  get parametrosDaSecaoAtual(): ParametroSistema[] {
    return this.obterParametrosSecao(this.secaoAtiva);
  }

  get pendencias(): ParametroSistema[] {
    return this.parametros.filter(parametro => this.isChanged(parametro) || this.isSectionChanged(parametro));
  }

  get pendenciasCount(): number {
    return this.pendencias.length;
  }

  carregarParametros(): void {
    this.carregando = true;
    this.parametroService.obterTodos().subscribe({
      next: (parametros) => {
        this.parametros = parametros
          .map(parametro => ({ ...parametro, secao: parametro.secao || 'Outros' }))
          .sort((a, b) => a.chave.localeCompare(b.chave));
        this.valoresOriginais = Object.fromEntries(this.parametros.map(parametro => [parametro.id, `${parametro.valor ?? ''}|||${parametro.secao || 'Outros'}`]));
        this.secoesExistentes = this.extrairSecoes(this.parametros);
        this.garantirSecaoValida();
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao carregar parâmetros', err);
        this.alertService.error('Erro', 'Erro ao carregar parâmetros');
        this.carregando = false;
      }
    });
  }

  contarParametrosSecao(secaoId: string): number {
    return this.obterParametrosSecao(secaoId).length;
  }

  selecionarSecao(secaoId: string): void {
    this.secaoAtiva = secaoId;
  }

  obterParametrosSecao(secaoId: string): ParametroSistema[] {
    return this.parametrosFiltrados.filter(parametro => (parametro.secao || 'Outros') === secaoId);
  }

  garantirSecaoValida(): void {
    const secaoVisivel = this.secoesVisiveis.find(secao => secao.id === this.secaoAtiva);
    if (!secaoVisivel && this.secoesVisiveis.length > 0) {
      this.secaoAtiva = this.secoesVisiveis[0].id;
    }
  }

  isChanged(parametro: ParametroSistema): boolean {
    const original = this.valoresOriginais[parametro.id] || '';
    return original.split('|||')[0] !== (parametro.valor ?? '');
  }

  isSectionChanged(parametro: ParametroSistema): boolean {
    const original = this.valoresOriginais[parametro.id] || '';
    return (original.split('|||')[1] || 'Outros') !== (parametro.secao || 'Outros');
  }

  async salvarTudo(): Promise<void> {
    if (this.pendenciasCount === 0) {
      return;
    }

    this.salvandoTudo = true;
    try {
      for (const parametro of this.pendencias) {
        await firstValueFrom(this.parametroService.atualizar(parametro.id, {
          valor: String(parametro.valor ?? ''),
          secao: parametro.secao || 'Outros'
        }));
        this.valoresOriginais[parametro.id] = `${parametro.valor ?? ''}|||${parametro.secao || 'Outros'}`;
      }

      this.alertService.success('Sucesso', 'Configurações salvas com sucesso.');
      this.salvandoTudo = false;
      this.carregarParametros();
    } catch (err: any) {
      console.error('Erro ao salvar parâmetros', err);
      const mensagem = err?.error?.mensagem || err?.message || 'Erro desconhecido';
      this.alertService.error('Erro', `Erro ao salvar alterações: ${mensagem}`);
      this.salvandoTudo = false;
    }
  }

  descartarAlteracoes(): void {
    this.parametros = this.parametros.map(parametro => {
      const [valor, secao] = (this.valoresOriginais[parametro.id] || '|||Outros').split('|||');
      return { ...parametro, valor: valor ?? '', secao: secao || 'Outros' };
    });
  }

  abrirModalNovo(): void {
    this.novoParametro = {
      chave: '',
      valor: '',
      descricao: '',
      secao: this.secaoAtiva || 'Outros',
      novaSecao: ''
    };
    this.usarNovaSecao = false;
    this.mostrarModal = true;
  }

  fecharModal(): void {
    this.mostrarModal = false;
    this.novoParametro = { chave: '', valor: '', descricao: '', secao: 'Outros', novaSecao: '' };
    this.usarNovaSecao = false;
  }

  fecharModalSeBackdrop(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.fecharModal();
    }
  }

  criarParametro(): void {
    this.criando = true;
    const dados = {
      chave: String(this.novoParametro.chave),
      valor: String(this.novoParametro.valor),
      descricao: this.novoParametro.descricao ? String(this.novoParametro.descricao) : undefined,
      secao: this.obterSecaoNovoParametro()
    };

    this.parametroService.criar(dados).subscribe({
      next: () => {
        this.criando = false;
        this.fecharModal();
        this.alertService.success('Sucesso', 'Parâmetro criado com sucesso.');
        this.carregarParametros();
      },
      error: (err: any) => {
        console.error('Erro ao criar parâmetro', err);
        const mensagem = err?.error?.mensagem || err?.error?.message || 'Erro desconhecido';
        this.alertService.error('Erro', `Erro ao criar parâmetro: ${mensagem}`);
        this.criando = false;
      }
    });
  }

  obterSecaoNovoParametro(): string {
    return this.usarNovaSecao
      ? (this.novoParametro.novaSecao || 'Outros').trim()
      : (this.novoParametro.secao || 'Outros').trim();
  }

  extrairSecoes(parametros: ParametroSistema[]): string[] {
    const secoes = Array.from(new Set(parametros.map(parametro => parametro.secao || 'Outros'))).sort((a, b) => a.localeCompare(b));
    return secoes.includes('Outros') ? secoes : [...secoes, 'Outros'];
  }

  getCampoTipo(parametro: ParametroSistema): string {
    if (parametro.chave === 'TipoEnderecoEntrega') return 'select-tipo-entrega';
    if (parametro.chave === 'FreteProvider') return 'select-frete-provider';
    if (['FreteHabilitado', 'MelhorEnvioNaoComercial'].includes(parametro.chave) || parametro.tipo === 'Boolean') return 'toggle';
    if (parametro.chave.startsWith('CarrosselImagem')) return 'url';
    if (['MaximoParcelas', 'FretePrazoPreparacaoDias'].includes(parametro.chave) || parametro.tipo === 'Numero') return 'number';
    if (parametro.chave === 'FreteCepOrigem' || parametro.tipo === 'Cep') return 'cep';
    return 'text';
  }

  getLabel(parametro: ParametroSistema): string {
    const labels: Record<string, string> = {
      TipoEnderecoEntrega: 'Tipo de Endereço de Entrega',
      FreteHabilitado: 'Frete Dinâmico Ativo',
      FreteProvider: 'Provedor de Frete',
      FreteCepOrigem: 'CEP de Origem',
      FretePrazoPreparacaoDias: 'Prazo de Preparação',
      MelhorEnvioNaoComercial: 'Envio Não Comercial',
      MelhorEnvioRemetenteNome: 'Nome do Remetente',
      MelhorEnvioRemetenteTelefone: 'Telefone do Remetente',
      MelhorEnvioRemetenteEmail: 'Email do Remetente',
      MelhorEnvioRemetenteDocumento: 'CPF/CNPJ do Remetente',
      MelhorEnvioRemetenteInscricaoEstadual: 'Inscrição Estadual',
      MelhorEnvioRemetenteLogradouro: 'Logradouro do Remetente',
      MelhorEnvioRemetenteNumero: 'Número do Remetente',
      MelhorEnvioRemetenteComplemento: 'Complemento do Remetente',
      MelhorEnvioRemetenteBairro: 'Bairro do Remetente',
      MelhorEnvioRemetenteCidade: 'Cidade do Remetente',
      MelhorEnvioRemetenteEstado: 'UF do Remetente',
      EmailAdministrador: 'Email do Administrador',
      MaximoParcelas: 'Máximo de Parcelas',
      CarrosselImagem1: 'Imagem 1 do Carrossel',
      CarrosselImagem2: 'Imagem 2 do Carrossel',
      CarrosselImagem3: 'Imagem 3 do Carrossel',
      CarrosselImagem4: 'Imagem 4 do Carrossel'
    };

    return labels[parametro.chave] || parametro.chave;
  }

  getPlaceholder(parametro: ParametroSistema): string {
    const placeholders: Record<string, string> = {
      FreteCepOrigem: '00000-000',
      FretePrazoPreparacaoDias: 'Ex: 2',
      MaximoParcelas: 'Ex: 3',
      MelhorEnvioRemetenteDocumento: 'CPF ou CNPJ',
      MelhorEnvioRemetenteTelefone: 'DDD + número',
      MelhorEnvioRemetenteEstado: 'Ex: SP',
      MelhorEnvioRemetenteNumero: 'Ex: 123',
      EmailAdministrador: '[email protected]'
    };

    return placeholders[parametro.chave] || 'Informe um valor';
  }

  getIconeSecao(secao: string): string {
    const nome = secao.toLowerCase();
    if (nome.includes('entrega')) return 'bi-geo-alt';
    if (nome.includes('frete')) return 'bi-truck';
    if (nome.includes('melhor envio')) return 'bi-box-seam';
    if (nome.includes('pagamento')) return 'bi-credit-card';
    if (nome.includes('home')) return 'bi-images';
    if (nome.includes('email')) return 'bi-envelope';
    return 'bi-wrench-adjustable-circle';
  }

  getDescricaoSecao(secao: string): string {
    const nome = secao.toLowerCase();
    if (nome.includes('entrega')) return 'Configura como o endereço de entrega é tratado no checkout.';
    if (nome.includes('frete')) return 'Controla a regra de cálculo, origem e prazo adicional da entrega.';
    if (nome.includes('melhor envio')) return 'Dados do remetente e opções logísticas para geração da etiqueta.';
    if (nome.includes('pagamento')) return 'Define regras financeiras e de parcelamento da loja.';
    if (nome.includes('home')) return 'Controla imagens e conteúdo visual da página inicial.';
    if (nome.includes('email')) return 'Parâmetros usados em notificações e comunicações.';
    return 'Parâmetros agrupados nesta seção.';
  }
}
