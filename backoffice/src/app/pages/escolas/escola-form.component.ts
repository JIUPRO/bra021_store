import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { EscolaService } from '../../services/escola.service';
import { AlertService } from '../../services/alert.service';
import { Escola, CriarEscola, AtualizarEscola } from '../../models/escola.model';

@Component({
  selector: 'app-escola-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container-fluid py-4">
      <div class="d-flex align-items-center mb-4">
        <a routerLink="/escolas" class="btn btn-outline-secondary me-3">
          <i class="bi bi-arrow-left"></i>
        </a>
        <h1 class="fw-bold mb-0">
          <i class="bi bi-building me-2"></i>
          {{ modoEdicao ? 'Editar Escola' : 'Nova Escola' }}
        </h1>
      </div>

      <div class="row">
        <div class="col-lg-8">
          <div class="card">
            <div class="card-body">
              <form (ngSubmit)="salvar()">
                <div class="mb-3">
                  <label class="form-label">Nome da Escola *</label>
                  <input type="text" class="form-control" [(ngModel)]="dados.nome" name="nome" required>
                </div>

                <h5 class="mt-4 mb-3">Endereço</h5>
                
                <div class="row">
                  <div class="col-md-3 mb-3">
                    <label class="form-label">CEP</label>
                    <input type="text" class="form-control" [(ngModel)]="dados.cep" name="cep" placeholder="00000-000" maxlength="9">
                  </div>

                  <div class="col-md-9 mb-3">
                    <label class="form-label">Logradouro</label>
                    <input type="text" class="form-control" [(ngModel)]="dados.logradouro" name="logradouro" placeholder="Rua, Avenida, etc.">
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-3 mb-3">
                    <label class="form-label">Número</label>
                    <input type="text" class="form-control" [(ngModel)]="dados.numero" name="numero">
                  </div>

                  <div class="col-md-3 mb-3">
                    <label class="form-label">Complemento</label>
                    <input type="text" class="form-control" [(ngModel)]="dados.complemento" name="complemento">
                  </div>

                  <div class="col-md-6 mb-3">
                    <label class="form-label">Bairro</label>
                    <input type="text" class="form-control" [(ngModel)]="dados.bairro" name="bairro">
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-8 mb-3">
                    <label class="form-label">Cidade</label>
                    <input type="text" class="form-control" [(ngModel)]="dados.cidade" name="cidade">
                  </div>

                  <div class="col-md-4 mb-3">
                    <label class="form-label">Estado</label>
                    <input type="text" class="form-control" [(ngModel)]="dados.estado" name="estado" placeholder="UF" maxlength="2">
                  </div>
                </div>

                <h5 class="mt-4 mb-3">Contato</h5>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Telefone/Email</label>
                    <input type="text" class="form-control" [(ngModel)]="dados.contato" name="contato" placeholder="Telefone ou email">
                  </div>

                  <div class="col-md-6 mb-3">
                    <label class="form-label">Professor Responsável</label>
                    <input type="text" class="form-control" [(ngModel)]="dados.professorResponsavel" name="professorResponsavel">
                  </div>
                </div>

                <h5 class="mt-4 mb-3">Comissão</h5>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Percentual de Comissão *</label>
                    <div class="input-group">
                      <input type="number" class="form-control" [(ngModel)]="dados.percentualComissao" name="percentualComissao" min="0" max="100" step="0.01" required>
                      <span class="input-group-text">%</span>
                    </div>
                    <small class="form-text text-muted">Ex: 10.50 para 10,50%</small>
                  </div>

                  <div class="col-md-6 mb-3" *ngIf="modoEdicao">
                    <label class="form-label">Status</label>
                    <select class="form-select" [(ngModel)]="dados.ativo" name="ativo">
                      <option [ngValue]="true">Ativa</option>
                      <option [ngValue]="false">Inativa</option>
                    </select>
                  </div>
                </div>

                <div class="d-flex gap-2">
                  <button type="submit" class="btn btn-primary" [disabled]="salvando">
                    <span *ngIf="!salvando">
                      <i class="bi bi-check-lg me-2"></i>Salvar
                    </span>
                    <span *ngIf="salvando">
                      <span class="spinner-border spinner-border-sm me-2"></span>
                      Salvando...
                    </span>
                  </button>
                  <a routerLink="/escolas" class="btn btn-outline-secondary">Cancelar</a>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class EscolaFormComponent implements OnInit {
  dados: any = {
    nome: '',
    cep: '',
    logradouro: '',
    numero: '',
    complemento: '',
    bairro: '',
    cidade: '',
    estado: '',
    contato: '',
    professorResponsavel: '',
    percentualComissao: 0,
    ativo: true
  };
  modoEdicao = false;
  escolaId?: string;
  salvando = false;

  constructor(
    private escolaService: EscolaService,
    private router: Router,
    private route: ActivatedRoute,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.escolaId = this.route.snapshot.paramMap.get('id') || undefined;
    if (this.escolaId) {
      this.modoEdicao = true;
      this.carregarEscola();
    }
  }

  carregarEscola(): void {
    if (!this.escolaId) return;

    this.escolaService.obterPorId(this.escolaId).subscribe({
      next: (escola) => {
        this.dados = { ...escola };
      },
      error: (err) => {
        console.error('Erro ao carregar escola', err);
        this.alertService.error('Erro', 'Erro ao carregar escola');
      }
    });
  }

  salvar(): void {
    if (!this.dados.nome || this.dados.percentualComissao == null) {
      this.alertService.warning('Atenção', 'Preencha os campos obrigatórios');
      return;
    }

    this.salvando = true;

    if (this.modoEdicao && this.escolaId) {
      const atualizar: AtualizarEscola = {
        id: this.escolaId,
        ...this.dados
      };
      this.escolaService.atualizar(this.escolaId, atualizar).subscribe({
        next: () => {
          this.alertService.success('Sucesso', 'Escola atualizada com sucesso');
          this.router.navigate(['/escolas']);
        },
        error: (err) => {
          console.error('Erro ao atualizar escola', err);
          this.alertService.error('Erro', 'Erro ao atualizar escola');
          this.salvando = false;
        }
      });
    } else {
      const criar: CriarEscola = {
        nome: this.dados.nome,
        cep: this.dados.cep,
        logradouro: this.dados.logradouro,
        numero: this.dados.numero,
        complemento: this.dados.complemento,
        bairro: this.dados.bairro,
        cidade: this.dados.cidade,
        estado: this.dados.estado,
        contato: this.dados.contato,
        professorResponsavel: this.dados.professorResponsavel,
        percentualComissao: this.dados.percentualComissao
      };
      this.escolaService.criar(criar).subscribe({
        next: () => {
          this.alertService.success('Sucesso', 'Escola criada com sucesso');
          this.router.navigate(['/escolas']);
        },
        error: (err) => {
          console.error('Erro ao criar escola', err);
          this.alertService.error('Erro', 'Erro ao criar escola');
          this.salvando = false;
        }
      });
    }
  }
}
