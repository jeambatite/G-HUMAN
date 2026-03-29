import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RolService } from '../../services/rol.service';
import { PermisoService } from '../../services/permiso.service';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './roles.html',
  styleUrl: './roles.css',
})
export class Roles implements OnInit {

  roles: { id: number; nombre: string; permisos: string[]; idPermisos: number[] }[] = [];
  permisosDisponibles: { id: number; nombre: string }[] = [];
  notificacion: { mensaje: string; tipo: 'exito' | 'error' } | null = null;

  // Modal crear/editar rol
  modalRolAbierto: boolean = false;
  esNuevoRol: boolean = true;
  rolEnEdicion: { id?: number; nombre: string; permisos: number[] } | null = null;

  // Modal eliminar rol
  modalEliminarRolAbierto: boolean = false;
  rolAEliminar: { id?: number; nombre: string; permisos: number[] } | null = null;
  claveEliminacionRol: string = '';
  errorClaveRol: boolean = false;

  constructor(
    private authService: AuthService,
    private rolService: RolService,
    private permisoService: PermisoService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (!this.authService.estaAutenticado()) {
      this.router.navigate(['/']);
      return;
    }
    if (!this.authService.tienePermiso('Gestionar roles')) {
      this.router.navigate(['/dashboard']);
      return;
    }
    this.aplicarTema();
    this.cargarRoles();
    this.permisoService.getAll().subscribe({
      next: (data) => this.permisosDisponibles = data,
      error: (err) => console.error('Error cargando permisos', err)
    });

    setInterval(() => {
      if (this.authService.sesionExpirada()) {
        this.authService.cerrarSesion();
        this.router.navigate(['/']);
      }
    }, 60000);

    const resetTimer = () => this.authService.refreshSesion();
    document.addEventListener('click', resetTimer);
    document.addEventListener('keypress', resetTimer);
    document.addEventListener('mousemove', resetTimer);
  }

  aplicarTema(): void {
    const vars: Record<string, string> = {
      '--blue':       '#1d4ed8',
      '--blue-dark':  '#1e40af',
      '--blue-light': '#3b82f6',
      '--blue-bg':    '#eff6ff',
      '--blue-bd':    '#bfdbfe',
      '--green':      '#16a34a',
      '--green-bg':   '#f0fdf4',
      '--green-bd':   '#bbf7d0',
      '--red':        '#dc2626',
      '--red-bg':     '#fef2f2',
      '--red-bd':     '#fecaca',
      '--white':      '#ffffff',
      '--gray-50':    '#f8fafc',
      '--gray-100':   '#f1f5f9',
      '--gray-200':   '#e2e8f0',
      '--gray-400':   '#64748b',
      '--gray-600':   '#334155',
      '--gray-900':   '#0f172a',
      '--accent':     '#16a34a',
      '--accent-bg':  '#f0fdf4',
      '--accent-bd':  '#bbf7d0',
    };
    Object.entries(vars).forEach(([key, value]) => {
      document.documentElement.style.setProperty(key, value);
    });
  }

  tienePermiso(permiso: string): boolean {
    return this.authService.tienePermiso(permiso);
  }

  cargarRoles(): void {
    this.rolService.getAll().subscribe({
      next: (data) => { this.roles = data; this.cdr.detectChanges(); },
      error: (err) => console.error('Error cargando roles', err)
    });
  }

  mostrarNotificacion(mensaje: string, tipo: 'exito' | 'error' = 'exito'): void {
    this.notificacion = { mensaje, tipo };
    this.cdr.detectChanges();
    setTimeout(() => { this.notificacion = null; this.cdr.detectChanges(); }, 3000);
  }

  abrirModalCrearRol(): void {
    this.rolEnEdicion = { nombre: '', permisos: [] };
    this.esNuevoRol = true;
    this.modalRolAbierto = true;
  }

  abrirModalEditarRol(rol: any): void {
    this.rolEnEdicion = { id: rol.id, nombre: rol.nombre, permisos: [...(rol.idPermisos ?? [])] };
    this.esNuevoRol = false;
    this.modalRolAbierto = true;
  }

  cerrarModalRol(): void {
    this.modalRolAbierto = false;
    this.rolEnEdicion = null;
  }

  togglePermiso(id: number): void {
    if (!this.rolEnEdicion) return;
    const idx = this.rolEnEdicion.permisos.indexOf(id);
    if (idx === -1) this.rolEnEdicion.permisos.push(id);
    else this.rolEnEdicion.permisos.splice(idx, 1);
  }

  guardarRol(): void {
    if (!this.rolEnEdicion || !this.rolEnEdicion.nombre.trim()) {
      alert('El nombre del rol es obligatorio.');
      return;
    }
    if (this.esNuevoRol) {
      this.rolService.crear({
        nombre: this.rolEnEdicion.nombre,
        idPermisos: this.rolEnEdicion.permisos
      }).subscribe({
        next: () => { this.cargarRoles(); this.cerrarModalRol(); this.mostrarNotificacion('Rol creado correctamente.'); },
        error: (err) => console.error('Error creando rol', err)
      });
    } else {
      this.rolService.editar(this.rolEnEdicion.id!, {
        idPermisos: this.rolEnEdicion.permisos
      }).subscribe({
        next: () => { this.cargarRoles(); this.cerrarModalRol(); this.mostrarNotificacion('Rol actualizado correctamente.'); },
        error: (err) => console.error('Error editando rol', err)
      });
    }
  }

  abrirModalEliminarRol(rol: any): void {
    this.rolAEliminar = rol;
    this.claveEliminacionRol = '';
    this.errorClaveRol = false;
    this.modalEliminarRolAbierto = true;
  }

  cerrarModalEliminarRol(): void {
    this.modalEliminarRolAbierto = false;
    this.rolAEliminar = null;
    this.claveEliminacionRol = '';
  }

  confirmarEliminarRol(): void {
    if (this.claveEliminacionRol.toLowerCase() !== 'confirmar') {
      this.errorClaveRol = true;
      return;
    }
    if (!this.rolAEliminar?.id) return;
    this.rolService.eliminar(this.rolAEliminar.id).subscribe({
      next: () => { this.cargarRoles(); this.cerrarModalEliminarRol(); this.mostrarNotificacion('Rol eliminado correctamente.'); },
      error: (err) => console.error('Error eliminando rol', err)
    });
  }

  logout(): void {
    this.authService.cerrarSesion();
    this.router.navigate(['/']);
  }

  get currentRole(): string { return this.authService.getRol() ?? ''; }

  get user() {
    return {
      name: this.authService.getNombre() ?? '',
      initials: (this.authService.getNombre() ?? 'U').substring(0, 2).toUpperCase(),
      badge: this.currentRole,
      roleLabel: this.currentRole
    };
  }
}
