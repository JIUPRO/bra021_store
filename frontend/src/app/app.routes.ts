import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'produtos',
    loadComponent: () => import('./pages/produtos/produtos.component').then(m => m.ProdutosComponent)
  },
  {
    path: 'produto/:id',
    loadComponent: () => import('./pages/produto-detalhe/produto-detalhe.component').then(m => m.ProdutoDetalheComponent)
  },
  {
    path: 'categoria/:id',
    loadComponent: () => import('./pages/produtos/produtos.component').then(m => m.ProdutosComponent)
  },
  {
    path: 'carrinho',
    loadComponent: () => import('./pages/carrinho/carrinho.component').then(m => m.CarrinhoComponent)
  },
  {
    path: 'checkout',
    loadComponent: () => import('./pages/checkout/checkout.component').then(m => m.CheckoutComponent),
    canActivate: [authGuard]
  },
  {
    path: 'pagamento',
    loadComponent: () => import('./pages/pagamento/pagamento.component').then(m => m.PagamentoComponent),
    canActivate: [authGuard]
  },
  {
    path: 'pedido-sucesso',
    loadComponent: () => import('./pages/pedido-sucesso/pedido-sucesso.component').then(m => m.PedidoSucessoComponent),
    canActivate: [authGuard]
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'cadastro',
    loadComponent: () => import('./pages/cadastro/cadastro.component').then(m => m.CadastroComponent)
  },
  {
    path: 'meus-pedidos',
    loadComponent: () => import('./pages/meus-pedidos/meus-pedidos.component').then(m => m.MeusPedidosComponent),
    canActivate: [authGuard]
  },
  {
    path: 'pedido-detalhe/:id',
    loadComponent: () => import('./pages/pedido-detalhe/pedido-detalhe.component').then(m => m.PedidoDetalheComponent),
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
