import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService, Usuario } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  template: `
    <div class="usuarios-page">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold mb-0">
          <i class="bi bi-people-fill me-2"></i>Gerenciar Usuários
        </h2>
        <button class="btn btn-primary" (click)="abrirNovoUsuario()">
          <i class="bi bi-plus-circle me-2"></i>Novo Usuário
        </button>
      </div>

      <!-- Tabela de Usuários -->
      <div class="card card-dashboard">
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-hover mb-0">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Email</th>
                  <th>Status</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let usuario of usuariosPaginados">
                  <td><strong>{{ usuario.nome }}</strong></td>
                  <td>{{ usuario.email }}</td>
                  <td>
                    <span class="badge" [class]="usuario.ativo ? 'bg-success' : 'bg-danger'">
                      {{ usuario.ativo ? 'Ativo' : 'Inativo' }}
                    </span>
                  </td>
                  <td>
                    <div class="btn-group">
                      <button class="btn btn-sm btn-outline-primary" (click)="editarUsuario(usuario)">
                        <i class="bi bi-pencil"></i>
                      </button>
                      <button class="btn btn-sm btn-outline-danger" (click)="deletarUsuario(usuario)">
                        <i class="bi bi-trash"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div class="card-footer bg-white">
          <app-pagination
            [totalItens]="usuarios.length"
            [paginaAtual]="paginaAtual"
            [itensPorPagina]="itensPorPagina"
            (paginar)="onPaginar($event)">
          </app-pagination>
        </div>
      </div>

      <!-- Modal de Novo/Edição Usuário -->
      <div class="modal" [class.show]="mostrarModal" [style.display]="mostrarModal ? 'block' : 'none'">
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">
                {{ usuarioEditando ? 'Editar Usuário' : 'Novo Usuário' }}
              </h5>
              <button type="button" class="btn-close" (click)="fecharModal()"></button>
            </div>
            <form (ngSubmit)="salvarUsuario()">
              <div class="modal-body">
                <div class="form-group mb-3">
                  <label for="nome" class="form-label">Nome</label>
                  <input
                    type="text"
                    class="form-control"
                    id="nome"
                    [(ngModel)]="formulario.nome"
                    name="nome"
                    required
                  />
                </div>

                <div class="form-group mb-3">
                  <label for="email" class="form-label">Email</label>
                  <input
                    type="email"
                    class="form-control"
                    id="email"
                    [(ngModel)]="formulario.email"
                    name="email"
                    [disabled]="!!usuarioEditando"
                    required
                  />
                </div>

                <div class="form-group mb-3" *ngIf="!usuarioEditando">
                  <label for="senha" class="form-label">Senha</label>
                  <input
                    type="password"
                    class="form-control"
                    id="senha"
                    [(ngModel)]="formulario.senha"
                    name="senha"
                    required
                  />
                </div>

                <div class="form-group mb-0">
                  <div class="form-check">
                    <input
                      type="checkbox"
                      class="form-check-input"
                      id="ativo"
                      [(ngModel)]="formulario.ativo"
                      name="ativo"
                    />
                    <label class="form-check-label" for="ativo">
                      Usuário Ativo
                    </label>
                  </div>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" (click)="fecharModal()">
                  Cancelar
                </button>
                <button type="submit" class="btn btn-primary" [disabled]="carregando">
                  <ng-container *ngIf="!carregando">
                    <i class="bi bi-save me-2"></i>Salvar
                  </ng-container>
                  <ng-container *ngIf="carregando">
                    <span class="spinner-border spinner-border-sm me-2"></span>Salvando...
                  </ng-container>
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <div class="modal-backdrop fade" [class.show]="mostrarModal" *ngIf="mostrarModal"></div>
    </div>
  `,
  styles: [`
    .card-dashboard {
      border: none;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
    }

    .table th {
      font-weight: 600;
      background: #f8f9fa;
      border: none;
    }

    .table td {
      vertical-align: middle;
    }

    .modal-content {
      border-radius: 12px;
      border: none;
    }
  `]
})
export class UsuariosComponent implements OnInit {
  private authService = inject(AuthService);
  private alertService = inject(AlertService);

  usuarios: Usuario[] = [];
  usuariosPaginados: Usuario[] = [];
  paginaAtual = 1;
  itensPorPagina = 10;
  mostrarModal = false;
  carregando = false;
  usuarioEditando: Usuario | null = null;

  formulario = {
    nome: '',
    email: '',
    senha: '',
    ativo: true
  };

  ngOnInit(): void {
    this.carregarUsuarios();
  }

  carregarUsuarios(): void {
    this.authService.listarUsuarios().subscribe({
      next: (usuarios) => {
        this.usuarios = usuarios;
        this.atualizarPaginacao();
      },
      error: (err) => {
        console.error('Erro ao carregar usuários', err);
        this.alertService.error('Erro', 'Não foi possível carregar os usuários');
      }
    });
  }

  abrirNovoUsuario(): void {
    this.usuarioEditando = null;
    this.formulario = { nome: '', email: '', senha: '', ativo: true };
    this.mostrarModal = true;
  }

  editarUsuario(usuario: Usuario): void {
    this.usuarioEditando = usuario;
    this.formulario = {
      nome: usuario.nome,
      email: usuario.email,
      senha: '',
      ativo: usuario.ativo
    };
    this.mostrarModal = true;
  }

  fecharModal(): void {
    this.mostrarModal = false;
    this.usuarioEditando = null;
    this.formulario = { nome: '', email: '', senha: '', ativo: true };
  }

  salvarUsuario(): void {
    if (!this.formulario.nome || !this.formulario.email) {
      this.alertService.warning('Aviso', 'Preencha os campos obrigatórios');
      return;
    }

    if (!this.usuarioEditando && !this.formulario.senha) {
      this.alertService.warning('Aviso', 'Digite uma senha para o novo usuário');
      return;
    }

    this.carregando = true;

    if (this.usuarioEditando) {
      // Editar usuário
      this.authService.atualizarUsuario(this.usuarioEditando.id, {
        nome: this.formulario.nome,
        ativo: this.formulario.ativo
      }).subscribe({
        next: () => {
          this.alertService.success('Sucesso', 'Usuário atualizado com sucesso');
          this.fecharModal();
          this.carregarUsuarios();
        },
        error: (err) => {
          console.error('Erro ao atualizar usuário', err);
          this.alertService.error('Erro', 'Não foi possível atualizar o usuário');
          this.carregando = false;
        }
      });
    } else {
      // Criar novo usuário
      this.authService.criarUsuario(this.formulario.email, this.formulario.nome, this.formulario.senha).subscribe({
        next: () => {
          this.alertService.success('Sucesso', 'Usuário criado com sucesso');
          this.fecharModal();
          this.carregarUsuarios();
        },
        error: (err) => {
          console.error('Erro ao criar usuário', err);
          this.alertService.error('Erro', 'Não foi possível criar o usuário');
          this.carregando = false;
        }
      });
    }
  }

  deletarUsuario(usuario: Usuario): void {
    if (confirm(`Tem certeza que deseja deletar o usuário ${usuario.nome}?`)) {
      this.authService.deletarUsuario(usuario.id).subscribe({
        next: () => {
          this.alertService.success('Sucesso', 'Usuário deletado com sucesso');
          this.carregarUsuarios();
        },
        error: (err) => {
          console.error('Erro ao deletar usuário', err);
          this.alertService.error('Erro', 'Não foi possível deletar o usuário');
        }
      });
    }
  }

  onPaginar(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtual = event.pagina;
    this.itensPorPagina = event.itensPorPagina;
    this.atualizarPaginacao();
  }

  private atualizarPaginacao(): void {
    const inicio = (this.paginaAtual - 1) * this.itensPorPagina;
    const fim = inicio + this.itensPorPagina;
    this.usuariosPaginados = this.usuarios.slice(inicio, fim);
  }
}
