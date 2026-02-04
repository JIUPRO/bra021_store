import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'produtos',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/produtos/produtos.component').then(m => m.ProdutosComponent)
  },
  {
    path: 'produtos/novo',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/produtos/produto-form/produto-form.component').then(m => m.ProdutoFormComponent)
  },
  {
    path: 'produtos/editar/:id',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/produtos/produto-form/produto-form.component').then(m => m.ProdutoFormComponent)
  },
  {
    path: 'categorias',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/categorias/categorias.component').then(m => m.CategoriasComponent)
  },
  {
    path: 'pedidos',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/pedidos/pedidos.component').then(m => m.PedidosComponent)
  },
  {
    path: 'pedidos/:id',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/pedidos/pedido-detalhe/pedido-detalhe.component').then(m => m.PedidoDetalheComponent)
  },
  {
    path: 'clientes',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/clientes/clientes.component').then(m => m.ClientesComponent)
  },
  {
    path: 'estoque',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/estoque/estoque.component').then(m => m.EstoqueComponent)
  },
  {
    path: 'escolas',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/escolas/escolas.component').then(m => m.EscolasComponent)
  },
  {
    path: 'escolas/nova',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/escolas/escola-form.component').then(m => m.EscolaFormComponent)
  },
  {
    path: 'escolas/editar/:id',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/escolas/escola-form.component').then(m => m.EscolaFormComponent)
  },
  {
    path: 'parametros',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/parametros/parametros.component').then(m => m.ParametrosComponent)
  },
  {
    path: 'usuarios',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/usuarios/usuarios.component').then(m => m.UsuariosComponent)
  },
  {
    path: 'relatorios',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/relatorios/relatorios.component').then(m => m.RelatoriosComponent)
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
