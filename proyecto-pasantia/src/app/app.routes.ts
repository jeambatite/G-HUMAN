import { Routes } from '@angular/router';
import { Login } from './login/login';
import { Dashboard } from './dashboard/dashboard';
import { Reportes } from './pages/reportes/reportes';
import { Empleados } from './pages/empleados/empleados';
import { Roles } from './pages/roles/roles';
import { authGuard } from './auth.guard';
import { AgregarPersonal } from './pages/agregar-personal/agregar-personal';
import { Nomina } from './pages/nomina/nomina';
export const routes: Routes = [
    { path: '', component: Login },
    {
        path: 'dashboard', component: Dashboard, canActivate: [authGuard], children: [
            
        ]
    },
    { path: 'dashboard/empleados', component: Empleados, canActivate: [authGuard] },
    { path: 'dashboard/roles', component: Roles, canActivate: [authGuard] },
    { path: 'dashboard/agregar', component: AgregarPersonal, canActivate: [authGuard] },
    { path: 'dashboard/reportes', component: Reportes, canActivate: [authGuard] },
    { path: 'dashboard/nomina', component: Nomina, canActivate: [authGuard] },


];
