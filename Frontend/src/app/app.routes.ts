import { Routes } from '@angular/router';
import { Login } from './login/login';
import { Register } from './register/register';
import { Dashboard } from './dashboard/dashboard';
import { authGuard } from './guards/auth-guard';
import { roleGuard } from './guards/role-guard';
import { Admin } from './admin/admin';




export const routes: Routes = [

  { path: '', redirectTo: 'login', pathMatch: 'full' },

  { path: 'login', component: Login },
  { path: 'register', component: Register },

  {
    path: 'dashboard',
    component: Dashboard // ✅ CUSTOMER PAGE
  },

  {
    path: 'admin',
    component: Admin, // ✅ ADMIN PAGE
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] }
  }

];