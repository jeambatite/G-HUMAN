import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ReclutamientoService } from '../../services/reclutamiento.service';

@Component({
  selector: 'app-reclutamiento',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './reclutamiento.html',
  styleUrl: './reclutamiento.css',
})
export class Reclutamiento implements OnInit {

  postulantes: any[] = [];
  filtrosAts: any[] = [];
  notificacion: { mensaje: string; tipo: 'exito' | 'error' } | null = null;

  // Filtros de búsqueda
  filtros = { estado: '', puesto: '', busquedaCv: '' };
  private filtroTimeout: any = null;

  // Paginación
  paginaActual: number = 1;
  tamanoPagina: number = 10;
  opcionesTamano: number[] = [10, 25, 50];

  // Modal estado
  modalEstadoAbierto: boolean = false;
  postulanteSeleccionado: any = null;
  nuevoEstado: string = '';
  notasEstado: string = '';

  // Modal filtro ATS
  modalFiltroAbierto: boolean = false;
  nuevoFiltro = { nombre: '', palabrasClave: '' };

  // Vista
  vistaActiva: 'postulantes' | 'filtros' = 'postulantes';
  mostrarSoloCoincidencias: boolean = false;

  constructor(
    private authService: AuthService,
    private reclutamientoService: ReclutamientoService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (!this.authService.estaAutenticado()) {
      this.router.navigate(['/']);
      return;
    }
    if (!this.authService.tienePermiso('Reclutamiento')) {
      this.router.navigate(['/dashboard']);
      return;
    }
    //this.aplicarTema();
    this.cargarPostulantes();
    this.cargarFiltros();

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
 /*aplicarTema(): void {
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
  }*/

  tienePermiso(permiso: string): boolean {
    return this.authService.tienePermiso(permiso);
  }

  cargarPostulantes(): void {
    const busqueda = this.mostrarSoloCoincidencias ? undefined : this.filtros.busquedaCv;
    this.reclutamientoService.getPostulantes(
      this.filtros.estado || undefined,
      this.filtros.puesto || undefined,
      busqueda || undefined
    ).subscribe({
      next: (data) => { this.postulantes = data; this.paginaActual = 1; this.cdr.detectChanges(); },
      error: (err) => console.error('Error cargando postulantes', err)
    });
  }

  cargarFiltros(): void {
    this.reclutamientoService.getFiltros().subscribe({
      next: (data) => { this.filtrosAts = data; this.cdr.detectChanges(); },
      error: (err) => console.error('Error cargando filtros', err)
    });
  }

  onFiltroChange(): void {
    if (this.filtroTimeout) clearTimeout(this.filtroTimeout);
    this.filtroTimeout = setTimeout(() => this.cargarPostulantes(), 1000);
  }

  limpiarFiltros(): void {
    this.filtros = { estado: '', puesto: '', busquedaCv: '' };
    this.cargarPostulantes();
  }

  hayFiltrosActivos(): boolean {
    return Object.values(this.filtros).some(v => v !== '');
  }

  aplicarFiltrosAts(): void {
    this.mostrarSoloCoincidencias = true;
    this.reclutamientoService.aplicarFiltros().subscribe({
      next: (data) => { this.postulantes = data; this.paginaActual = 1; this.cdr.detectChanges(); },
      error: (err) => console.error('Error aplicando filtros ATS', err)
    });
  }

  mostrarTodos(): void {
    this.mostrarSoloCoincidencias = false;
    this.cargarPostulantes();
  }

  // Paginación
  get postulantesPaginados(): any[] {
    const inicio = (this.paginaActual - 1) * this.tamanoPagina;
    return this.postulantes.slice(inicio, inicio + this.tamanoPagina);
  }

  get totalPaginas(): number {
    return Math.ceil(this.postulantes.length / this.tamanoPagina);
  }

  get paginas(): (number | string)[] {
    const total = this.totalPaginas;
    if (total <= 15) return Array.from({ length: total }, (_, i) => i + 1);
    const result: (number | string)[] = [];
    for (let i = 1; i <= 10; i++) result.push(i);
    if (this.paginaActual > 10 && this.paginaActual <= total - 5) {
      result.push('...');
      for (let i = this.paginaActual - 1; i <= this.paginaActual + 1; i++) result.push(i);
    }
    result.push('...');
    for (let i = total - 4; i <= total; i++) result.push(i);
    return [...new Set(result)];
  }

  cambiarPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas) return;
    this.paginaActual = pagina;
    this.cdr.detectChanges();
  }

  // Modal estado
  abrirModalEstado(p: any): void {
    this.postulanteSeleccionado = p;
    this.nuevoEstado = p.estado;
    this.notasEstado = p.notas ?? '';
    this.modalEstadoAbierto = true;
  }

  cerrarModalEstado(): void {
    this.modalEstadoAbierto = false;
    this.postulanteSeleccionado = null;
  }

  guardarEstado(): void {
    if (!this.postulanteSeleccionado) return;
    this.reclutamientoService.actualizarEstado(this.postulanteSeleccionado.id, this.nuevoEstado, this.notasEstado).subscribe({
      next: () => {
        this.postulanteSeleccionado.estado = this.nuevoEstado;
        this.postulanteSeleccionado.notas = this.notasEstado;
        this.cerrarModalEstado();
        this.mostrarNotificacion('Estado actualizado correctamente.');
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error actualizando estado', err)
    });
  }

  eliminarPostulante(p: any): void {
    if (!confirm(`¿Eliminar a ${p.nombre}?`)) return;
    this.reclutamientoService.eliminarPostulante(p.id).subscribe({
      next: () => {
        this.postulantes = this.postulantes.filter(x => x.id !== p.id);
        this.mostrarNotificacion('Postulante eliminado.');
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error eliminando postulante', err)
    });
  }

  // Modal filtro ATS
  abrirModalFiltro(): void {
    this.nuevoFiltro = { nombre: '', palabrasClave: '' };
    this.modalFiltroAbierto = true;
  }

  cerrarModalFiltro(): void {
    this.modalFiltroAbierto = false;
  }

  guardarFiltro(): void {
    if (!this.nuevoFiltro.nombre || !this.nuevoFiltro.palabrasClave) {
      this.mostrarNotificacion('Nombre y palabras clave son obligatorios.', 'error');
      return;
    }
    this.reclutamientoService.crearFiltro(this.nuevoFiltro.nombre, this.nuevoFiltro.palabrasClave).subscribe({
      next: (data) => {
        this.filtrosAts.unshift(data);
        this.cerrarModalFiltro();
        this.mostrarNotificacion('Filtro creado correctamente.');
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error creando filtro', err)
    });
  }

  eliminarFiltro(id: number): void {
    this.reclutamientoService.eliminarFiltro(id).subscribe({
      next: () => {
        this.filtrosAts = this.filtrosAts.filter(f => f.id !== id);
        this.mostrarNotificacion('Filtro eliminado.');
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error eliminando filtro', err)
    });
  }

  toggleFiltro(filtro: any): void {
    this.reclutamientoService.toggleFiltro(filtro.id).subscribe({
      next: () => {
        filtro.activo = !filtro.activo;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error toggling filtro', err)
    });
  }

  getEstadoClass(estado: string): string {
    const map: Record<string, string> = {
      'pendiente':  'estado-vacaciones',
      'revisando':  'estado-activo',
      'aprobado':   'estado-activo',
      'rechazado':  'estado-inactivo',
      'contratado': 'estado-activo'
    };
    return map[estado] ?? 'estado-vacaciones';
  }

  mostrarNotificacion(mensaje: string, tipo: 'exito' | 'error' = 'exito'): void {
    this.notificacion = { mensaje, tipo };
    this.cdr.detectChanges();
    setTimeout(() => { this.notificacion = null; this.cdr.detectChanges(); }, 3000);
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
    };
  }
}