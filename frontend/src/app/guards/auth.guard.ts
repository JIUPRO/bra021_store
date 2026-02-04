import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { ClienteService } from '../services/cliente.service';

export const authGuard: CanActivateFn = (route, state) => {
  const clienteService = inject(ClienteService);
  const router = inject(Router);
  
  // Verificar se está logado
  if (!clienteService.estaLogado()) {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
  
  // Verificar se o token não expirou
  const tokenExpiracao = sessionStorage.getItem('tokenExpiracao');
  if (tokenExpiracao) {
    const agora = new Date().getTime();
    const expiracao = parseInt(tokenExpiracao, 10);
    
    if (agora >= expiracao) {
      // Token expirado
      clienteService.logout();
      router.navigate(['/login'], { queryParams: { returnUrl: state.url, expired: 'true' } });
      return false;
    }
  }
  
  return true;
};
