import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { EmpleadoService } from '../../services/empleado.service';
import { RolService } from '../../services/rol.service';
import { UsuarioService } from '../../services/usuario.service';

@Component({
  selector: 'app-empleados',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './empleados.html',
  styleUrl: './empleados.css',
})
export class Empleados implements OnInit {
  //variable
  paginaActual: number = 1;
  tamanoPagina: number = 10;
  totalEmpleados: number = 0;
  totalPaginas: number = 0;
  opcionesTamano: number[] = [10, 25, 50, 100];

  empleados: any[] = [];
  roles: any[] = [];
  filtros = { id: '', nombre: '', rol: '', estado: '', sueldo: '', departamento: '', jefe: '' };
  notificacion: { mensaje: string; tipo: 'exito' | 'error' } | null = null;

  // Modal editar
  modalAbierto: boolean = false;
  empleadoSeleccionado: any = null;

  // Modal eliminar
  modalEliminarAbierto: boolean = false;
  empleadoAEliminar: any = null;
  claveEliminacion: string = '';
  errorClave: boolean = false;

  // Modal sensible
  modalSensibleAbierto: boolean = false;
  empleadoSensible: any = null;

  // Modal crear usuario
  modalUsuarioAbierto: boolean = false;
  empleadoParaUsuario: any = null;
  nuevoUsuario = { username: '', password: '', pin: '' };
  errorUsuario: string = '';

  //temporizador filtro 
  private filtroTimeout: any = null;
  onFiltroChange(): void {
    if (this.filtroTimeout) clearTimeout(this.filtroTimeout);
    this.filtroTimeout = setTimeout(() => {
      this.paginaActual = 1;
      this.cargarEmpleados();
    }, 500);//2500=2.5 segundos
  }
  //
  constructor(
    private authService: AuthService,
    private empleadoService: EmpleadoService,
    private rolService: RolService,
    private usuarioService: UsuarioService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    if (!this.authService.estaAutenticado()) {
      this.router.navigate(['/']);
      return;
    }
    if (!this.authService.tienePermiso('Ver personal')) {
      this.router.navigate(['/dashboard']);
      return;
    }
    this.aplicarTema();
    this.cargarEmpleados();
    this.cargarRoles();

    // Verificar sesión cada minuto
    setInterval(() => {
      if (this.authService.sesionExpirada()) {
        this.authService.cerrarSesion();
        this.router.navigate(['/']);
      }
    }, 60000);

    // Reiniciar timer con cada acción del usuario
    const resetTimer = () => this.authService.refreshSesion();
    document.addEventListener('click', resetTimer);
    document.addEventListener('keypress', resetTimer);
    document.addEventListener('mousemove', resetTimer);
  }

  aplicarTema(): void {
    const vars: Record<string, string> = {
      '--blue': '#1d4ed8',
      '--blue-dark': '#1e40af',
      '--blue-light': '#3b82f6',
      '--blue-bg': '#eff6ff',
      '--blue-bd': '#bfdbfe',
      '--green': '#16a34a',
      '--green-bg': '#f0fdf4',
      '--green-bd': '#bbf7d0',
      '--red': '#dc2626',
      '--red-bg': '#fef2f2',
      '--red-bd': '#fecaca',
      '--white': '#ffffff',
      '--gray-50': '#f8fafc',
      '--gray-100': '#f1f5f9',
      '--gray-200': '#e2e8f0',
      '--gray-400': '#64748b',
      '--gray-600': '#334155',
      '--gray-900': '#0f172a',
      '--accent': '#16a34a',
      '--accent-bg': '#f0fdf4',
      '--accent-bd': '#bbf7d0',
    };
    Object.entries(vars).forEach(([key, value]) => {
      document.documentElement.style.setProperty(key, value);
    });
  }

  tienePermiso(permiso: string): boolean {
    return this.authService.tienePermiso(permiso);
  }

  cargarEmpleados(): void {
    this.empleadoService.getPaginado(this.paginaActual, this.tamanoPagina, this.filtros).subscribe({
      next: (data) => {
        this.empleados = data.data;
        this.totalEmpleados = data.total;
        this.totalPaginas = data.totalPaginas;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error cargando empleados', err)
    });
  }

  cargarRoles(): void {
    this.rolService.getAll().subscribe({
      next: (data) => { this.roles = data; this.cdr.detectChanges(); },
      error: (err) => console.error('Error cargando roles', err)
    });
  }

  get empleadosFiltrados(): any[] {
    return this.empleados.filter(e => {
      const matchId = !this.filtros.id || e.id.toString().includes(this.filtros.id.trim());
      const matchNombre = !this.filtros.nombre || e.nombre.toLowerCase().includes(this.filtros.nombre.toLowerCase().trim());
      const matchRol = !this.filtros.rol || e.nombreRol === this.filtros.rol;
      const matchEstado = !this.filtros.estado || e.estado === this.filtros.estado;
      const matchSueldo = !this.filtros.sueldo || e.sueldo >= Number(this.filtros.sueldo);
      const matchDepto = !this.filtros.departamento || e.departamento === this.filtros.departamento;
      const matchJefe = !this.filtros.jefe || e.nombreJefe === this.filtros.jefe;
      return matchId && matchNombre && matchRol && matchEstado && matchSueldo && matchDepto && matchJefe;
    });
  }

  get rolesUnicos(): string[] { return [...new Set(this.empleados.map(e => e.nombreRol).filter(Boolean))] as string[]; }
  get estadosUnicos(): string[] { return [...new Set(this.empleados.map(e => e.estado).filter(Boolean))] as string[]; }
  get deptosUnicos(): string[] { return [...new Set(this.empleados.map(e => e.departamento).filter(Boolean))] as string[]; }
  get jefesUnicos(): string[] { return [...new Set(this.empleados.map(e => e.nombreJefe).filter(Boolean))] as string[]; }

  hayFiltrosActivos(): boolean {
    return Object.values(this.filtros).some(v => v !== '');
  }

  limpiarFiltros(): void {
    this.filtros = { id: '', nombre: '', rol: '', estado: '', sueldo: '', departamento: '', jefe: '' };
    this.paginaActual = 1;
    this.cargarEmpleados();
  }

  mostrarNotificacion(mensaje: string, tipo: 'exito' | 'error' = 'exito'): void {
    this.notificacion = { mensaje, tipo };
    this.cdr.detectChanges();
    setTimeout(() => { this.notificacion = null; this.cdr.detectChanges(); }, 3000);
  }

  // Modal editar
  abrirModal(emp: any): void {
    this.empleadoSeleccionado = { ...emp, fechaI: this.formatearFecha(emp.fechaI ?? '') };
    this.modalAbierto = true;
  }

  cerrarModal(): void { this.modalAbierto = false; this.empleadoSeleccionado = null; }

  formatearFecha(fecha: string): string {
    if (!fecha) return '';
    if (/^\d{4}-\d{2}-\d{2}/.test(fecha)) return fecha.substring(0, 10);
    const partes = fecha.split('/');
    if (partes.length === 3) {
      const [dia, mes, anio] = partes;
      return `${anio}-${mes.padStart(2, '0')}-${dia.padStart(2, '0')}`;
    }
    return fecha;
  }

  guardarCambios(): void {
    if (!this.empleadoSeleccionado) return;
    const payload = { ...this.empleadoSeleccionado, idRol: this.empleadoSeleccionado.idRol };
    this.empleadoService.editar(this.empleadoSeleccionado.id, payload).subscribe({
      next: () => { this.cargarEmpleados(); this.cerrarModal(); this.mostrarNotificacion('Empleado actualizado correctamente.'); },
      error: (err) => console.error('Error editando empleado', err)
    });
  }

  // Modal eliminar
  abrirModalEliminar(emp: any): void {
    this.empleadoAEliminar = emp;
    this.claveEliminacion = '';
    this.errorClave = false;
    this.modalEliminarAbierto = true;
  }

  cerrarModalEliminar(): void { this.modalEliminarAbierto = false; this.empleadoAEliminar = null; this.claveEliminacion = ''; }

  confirmarEliminacion(): void {
    if (this.claveEliminacion.toLowerCase() !== 'confirmar') {
      this.errorClave = true;
      return;
    }
    this.empleadoService.eliminar(this.empleadoAEliminar!.id).subscribe({
      next: () => { this.cargarEmpleados(); this.cerrarModalEliminar(); this.mostrarNotificacion('Empleado eliminado correctamente.'); },
      error: (err) => console.error('Error eliminando empleado', err)
    });
  }

  // Modal sensible
  abrirModalSensible(emp: any): void {
    this.empleadoSensible = emp;
    this.modalSensibleAbierto = true;
    this.empleadoService.getDatosSensibles(emp.id).subscribe({
      next: (data) => { this.empleadoSensible = { ...emp, ...data }; this.cdr.detectChanges(); },
      error: (err) => console.error('Error cargando datos sensibles', err)
    });
  }

  cerrarModalSensible(): void { this.modalSensibleAbierto = false; this.empleadoSensible = null; }

  // Modal crear usuario
  abrirModalUsuario(emp: any): void {
    this.empleadoParaUsuario = emp;
    this.nuevoUsuario = { username: '', password: '', pin: '' };
    this.errorUsuario = '';
    this.modalUsuarioAbierto = true;
  }


  cerrarModalUsuario(): void { this.modalUsuarioAbierto = false; this.empleadoParaUsuario = null; }

  crearUsuario(): void {
    if (!this.nuevoUsuario.username || !this.nuevoUsuario.password) {
      this.errorUsuario = 'Usuario y contraseña son obligatorios.';
      return;
    }
    this.usuarioService.crear({
      idEmpleado: this.empleadoParaUsuario!.id,
      username: this.nuevoUsuario.username,
      password: this.nuevoUsuario.password,
      pin: this.nuevoUsuario.pin || null
    }).subscribe({
      next: () => { if (this.empleadoParaUsuario) this.empleadoParaUsuario.tieneUsuario = true; this.cerrarModalUsuario(); this.mostrarNotificacion('Usuario creado correctamente.'); },
      error: (err) => console.error('Error creando usuario', err)
    });
  }

  logout(): void {
    this.authService.cerrarSesion();
    this.router.navigate(['/']);
  }
  cambiarPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas) return;
    this.paginaActual = pagina;
    this.cargarEmpleados();
  }

  cambiarTamano(tamano: number): void {
    this.tamanoPagina = tamano;
    this.paginaActual = 1;
    this.cargarEmpleados();
  }

  get paginas(): (number | string)[] {
    const total = this.totalPaginas;
    const actual = this.paginaActual;
    const result: (number | string)[] = [];

    if (total <= 15) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    // Primeras 10
    for (let i = 1; i <= 10; i++) result.push(i);

    // Si la página actual está en el medio
    if (actual > 10 && actual <= total - 5) {
      result.push('...');
      for (let i = actual - 1; i <= actual + 1; i++) result.push(i);
    }

    result.push('...');

    // Últimas 5
    for (let i = total - 4; i <= total; i++) result.push(i);

    // Eliminar duplicados manteniendo orden
    return [...new Set(result)];
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