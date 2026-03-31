import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { EmpleadoService } from '../services/empleado.service';
import { RolService } from '../services/rol.service';
import { UsuarioService } from '../services/usuario.service';
import { RouterModule } from '@angular/router';
import { PermisoService } from '../services/permiso.service';

export interface Empleado {
  id: number;
  genero: string;
  nombre: string;
  rol: string;
  estado: 'activo' | 'vacaciones' | 'inactivo' | 'suspendido';
  email: string;
  sueldo: number;
  fecha_i: string;
  departamento: string;
  jefe: string;
  // Datos sensibles
  tipoDocumento?: string;
  numDocumento?: string;
  tipoSangre?: string;
  fechaNacimiento?: string;
  telefono?: string;
  contactoEmergencia?: string;
  telEmergencia?: string;
  tieneUsuario?: boolean;
  nombreRol?: string;
  nombreJefe?: string;
  idRol?: number;
  fechaI?: string;

}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {

  currentRole: string = 'n1';

  /*users: Record<string, any> = {
    n1: { name: 'Carlos Mendez', dept: 'Ventas', initials: 'CM', badge: 'Nivel 1 - Solo Lectura', roleLabel: 'Nivel 1' },
    n2: { name: 'Maria Lopez', dept: 'Administracion', initials: 'ML', badge: 'Nivel 2 - Edicion Basica', roleLabel: 'Nivel 2' },
    n3: { name: 'Roberto Silva', dept: 'Gerencia', initials: 'RS', badge: 'Nivel 3 - Acceso Total', roleLabel: 'Nivel 3' },
    admin: { name: 'Juan Diaz', dept: 'Recursos Humanos', initials: 'JD', badge: 'Administrador', roleLabel: 'Administrador' },
  };*/

  //modal cumleapños
  modalCumpleanosAbierto: boolean = false;
  cumpleaneros: any[] = [];

  get hoy(): string {
    const d = new Date();
    return `${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  verificarCumpleanos(): void {
    const hoy = this.hoy;
    
    this.cumpleaneros = this.empleados.filter(e => {
      if (!e.fechaNacimiento) return false;
      return e.fechaNacimiento.substring(5, 10) === hoy;
    });

    const ultimaVez = localStorage.getItem('cumpleanos_popup');
    console.log('ultimaVez:', ultimaVez, 'hoy:', hoy, 'son iguales:', ultimaVez === hoy);

    if (this.cumpleaneros.length > 0 && ultimaVez !== hoy) {
      this.modalCumpleanosAbierto = true;
      localStorage.setItem('cumpleanos_popup', hoy);
    }

    this.cdr.detectChanges();
  }

  abrirModalCumpleanos(): void {
    this.cumpleaneros = this.empleados.filter(e => {
      if (!e.fechaNacimiento) return false;
      const fecha = e.fechaNacimiento.substring(5, 10);
      return fecha === this.hoy;
    });
    this.modalCumpleanosAbierto = true;
  }

  cerrarModalCumpleanos(): void {
    this.modalCumpleanosAbierto = false;
  }
  //

  get user() {
    return {
      name: this.authService.getNombre() ?? '',
      dept: '',
      initials: (this.authService.getNombre() ?? 'U').substring(0, 2).toUpperCase(),
      badge: this.currentRole === 'Admin' ? 'Administrador' : this.currentRole,
      roleLabel: this.currentRole
    };
  }

  empleados: Empleado[] = [];

  filtros = { id: '', nombre: '', rol: '', estado: '', sueldo: '', departamento: '', jefe: '' };
  constructor(
    private authService: AuthService,
    private empleadoService: EmpleadoService,
    private rolService: RolService,
    private usuarioService: UsuarioService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private permisoService: PermisoService
  ) { }

  ngOnInit(): void {
    this.currentRole = this.authService.getRol() ?? '';
    this.aplicarTemaRol();
    this.cdr.detectChanges();
    this.cargarEmpleados();
    this.cargarRoles();
    this.permisoService.getAll().subscribe({
      next: (data) => this.permisosDisponibles = data,
      error: (err) => console.error('Error cargando permisos', err)
    });
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
  aplicarTemaRol(): void {
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
  getRolClass(): string {
    const nombre = this.currentRole.toLowerCase().trim();
    if (nombre.includes('nivel 1')) return 'role-n1';
    if (nombre.includes('nivel 2')) return 'role-n2';
    if (nombre.includes('nivel 3')) return 'role-n3';
    if (nombre.includes('admin')) return 'role-admin';
    return 'role-n1'; // color por defecto para roles nuevos
  }
  get totalVacaciones(): number {
    return this.empleados.filter(e => e.estado === 'vacaciones').length;
  }

  get totalSuspendidos(): number {
    return this.empleados.filter(e => e.estado === 'suspendido').length;
  }

  cargarEmpleados(): void {
    this.empleadoService.getAll().subscribe({
      next: (data) => {
        this.empleados = [...data];
        this.verificarCumpleanos();
        //this.modalCumpleanosAbierto = this.cumpleaneros.length > 0;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error cargando empleados', err)
    });
  }


  cargarRoles(): void {
    this.rolService.getAll().subscribe({
      next: (data) => {
        this.roles = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error cargando roles', err)
    });
  }

  get empleadosFiltrados(): Empleado[] {
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

  get rolesUnicos(): string[] { return [...new Set(this.empleados.map(e => e.nombreRol ?? e.rol).filter(Boolean))] as string[]; }
  get estadosUnicos(): string[] { return [...new Set(this.empleados.map(e => e.estado))]; }
  get deptosUnicos(): string[] { return [...new Set(this.empleados.map(e => e.departamento))]; }
  get jefesUnicos(): string[] { return [...new Set(this.empleados.map(e => e.nombreJefe ?? e.jefe).filter(Boolean))] as string[]; }

  // Departamentos disponibles (mock — vendrán del backend)
  departamentos: string[] = [
    'Ventas',
    'Administración',
    'Gerencia',
    'Tecnología',
    'Marketing',
    'Finanzas',
    'Recursos Humanos'
  ];
  // PERMISOS DISPONIBLES (mock — vendrán del backend)
  permisosDisponibles: { id: number; nombre: string }[] = [];
  //
  notificacion: { mensaje: string; tipo: 'exito' | 'error' } | null = null;

  // ROLES (mock — vendrán del backend)
  roles: { id: number; nombre: string; permisos: string[]; idPermisos: number[] }[] = [];
  //modales 
  modalEmpleadosAbierto: boolean = false;
  modalAgregarPersonalAbierto: boolean = false;
  modalRolesAbierto: boolean = false;
  abrirModalEmpleados(): void { this.modalEmpleadosAbierto = true; }
  cerrarModalEmpleados(): void { this.modalEmpleadosAbierto = false; }

  abrirModalAgregarPersonal(): void { this.modalAgregarPersonalAbierto = true; }
  cerrarModalAgregarPersonal(): void { this.modalAgregarPersonalAbierto = false; }

  abrirModalRoles(): void { this.modalRolesAbierto = true; }
  cerrarModalRoles(): void { this.modalRolesAbierto = false; }

  // MODAL CREAR/EDITAR ROL
  modalRolAbierto: boolean = false;
  rolEnEdicion: { id?: number; nombre: string; permisos: number[] } | null = null;
  esNuevoRol: boolean = false;

  abrirModalCrearRol(): void {
    this.rolEnEdicion = { nombre: '', permisos: [] };
    this.esNuevoRol = true;
    this.modalRolAbierto = true;
  }
  mostrarNotificacion(mensaje: string, tipo: 'exito' | 'error' = 'exito'): void {

    this.notificacion = { mensaje, tipo };
    this.cdr.detectChanges();
    setTimeout(() => {
      this.notificacion = null;
      this.cdr.detectChanges();
    }, 3000);
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
    if (idx === -1) {
      this.rolEnEdicion.permisos.push(id);
    } else {
      this.rolEnEdicion.permisos.splice(idx, 1);
    }
  }

  tienePermiso(permiso: string): boolean {
    return this.authService.tienePermiso(permiso);
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
        next: () => {
          this.cargarRoles();
          this.cerrarModalRol();
        },
        error: (err) => console.error('Error creando rol', err)
      });
    } else {
      this.rolService.editar(this.rolEnEdicion.id!, {
        idPermisos: this.rolEnEdicion.permisos
      }).subscribe({
        next: () => {
          this.cargarRoles();
          this.cerrarModalRol();
        },
        error: (err) => console.error('Error editando rol', err)
      });
    }
  }

  // ELIMINAR ROL
  modalEliminarRolAbierto: boolean = false;
  rolAEliminar: { id?: number; nombre: string; permisos: string[] } | null = null;
  claveEliminacionRol: string = '';
  errorClaveRol: boolean = false;

  abrirModalEliminarRol(rol: { nombre: string; permisos: string[] }): void {
    this.rolAEliminar = rol;
    this.claveEliminacionRol = '';
    this.errorClaveRol = false;
    this.modalEliminarRolAbierto = true;
  }
  //metodo arreglar fecha
  formatearFecha(fecha: string): string {
    if (!fecha) return '';
    // Si ya viene en formato YYYY-MM-DD
    if (/^\d{4}-\d{2}-\d{2}/.test(fecha)) {
      return fecha.substring(0, 10);
    }
    // Si viene en formato D/M/Y o DD/MM/YYYY
    const partes = fecha.split('/');
    if (partes.length === 3) {
      const [dia, mes, anio] = partes;
      return `${anio}-${mes.padStart(2, '0')}-${dia.padStart(2, '0')}`;
    }
    return fecha;
  }

  cerrarModalEliminarRol(): void {
    this.modalEliminarRolAbierto = false;
    this.rolAEliminar = null;
    this.claveEliminacionRol = '';
    this.errorClaveRol = false;
  }

  confirmarEliminarRol(): void {
    if (this.claveEliminacionRol.toLowerCase() !== 'confirmar') {
      this.errorClaveRol = true;
      return;
    }
    if (!this.rolAEliminar?.id) return;
    this.rolService.eliminar(this.rolAEliminar.id).subscribe({
      next: () => {
        this.cargarRoles();
        this.cerrarModalEliminarRol();
        this.mostrarNotificacion('Rol eliminado correctamente.');
      },
      error: (err) => console.error('Error eliminando rol', err)
    });
  }
  // Nuevo empleado
  nuevoEmpleado: Empleado = this.empleadoVacio();

  // MODAL DATOS SENSIBLES
  modalSensibleAbierto: boolean = false;
  empleadoSensible: Empleado | null = null;

  abrirModalSensible(emp: Empleado): void {
    this.empleadoSensible = emp;
    this.modalSensibleAbierto = true;
    this.empleadoService.getDatosSensibles(emp.id).subscribe({
      next: (data) => {
        this.empleadoSensible = { ...emp, ...data };
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error cargando datos sensibles', err)
    });
  }

  cerrarModalSensible(): void {
    this.modalSensibleAbierto = false;
    this.empleadoSensible = null;
  }

  // MODAL CREAR USUARIO
  modalUsuarioAbierto: boolean = false;
  empleadoParaUsuario: Empleado | null = null;
  nuevoUsuario = { username: '', password: '', pin: '' };
  errorUsuario: string = '';

  abrirModalUsuario(emp: Empleado): void {
    this.empleadoParaUsuario = emp;
    this.nuevoUsuario = { username: '', password: '', pin: '' };
    this.errorUsuario = '';
    this.modalUsuarioAbierto = true;
  }

  cerrarModalUsuario(): void {
    this.modalUsuarioAbierto = false;
    this.empleadoParaUsuario = null;
    this.errorUsuario = '';
  }

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
      next: () => {
        if (this.empleadoParaUsuario) this.empleadoParaUsuario.tieneUsuario = true;
        this.cerrarModalUsuario();
        this.mostrarNotificacion('Usuario creado correctamente.');
      },
      error: (err) => console.error('Error creando usuario', err)
    });
  }


  empleadoVacio(): Empleado {
    return {
      id: 0,
      genero: '',
      nombre: '',
      rol: '',
      estado: 'activo',
      email: '',
      sueldo: 0,
      fecha_i: '',
      fechaI: '',
      departamento: '',
      jefe: '',
      idRol: 0,
      tipoDocumento: '',
      numDocumento: '',
      tipoSangre: '',
      fechaNacimiento: '',
      telefono: '',
      contactoEmergencia: '',
      telEmergencia: '',
      tieneUsuario: false
    };
  }

  agregarEmpleado(): void {
    const e = this.nuevoEmpleado;
    if (
      !e.nombre || !e.email || !e.idRol || !e.genero ||
      !e.estado || !e.departamento || !e.fechaI ||
      !e.tipoDocumento || !e.numDocumento || !e.tipoSangre ||
      !e.fechaNacimiento || !e.telefono || !e.contactoEmergencia || !e.telEmergencia
    ) {
      alert('Todos los campos son obligatorios.');
      return;
    }

    this.empleadoService.crear(this.nuevoEmpleado).subscribe({
      next: () => {
        this.cargarEmpleados();
        this.nuevoEmpleado = this.empleadoVacio();
        this.mostrarNotificacion('Empleado agregado correctamente.');
      },
      error: (err) => {
        if (err.status === 409) {
          alert(err.error.message);
        } else {
          console.error('Error agregando empleado', err);
        }
      }
    });

  }

  limpiarFiltros(): void {
    this.filtros = { id: '', nombre: '', rol: '', estado: '', sueldo: '', departamento: '', jefe: '' };
  }

  hayFiltrosActivos(): boolean {
    return Object.values(this.filtros).some(v => v !== '');
  }

  // MODAL DE EDICION
  modalAbierto: boolean = false;
  empleadoSeleccionado: Empleado | null = null;

  abrirModal(emp: Empleado): void {
    //console.log('fecha original:', emp.fechaI, emp.fecha_i);
    this.empleadoSeleccionado = {
      ...emp,
      fechaI: this.formatearFecha(emp.fechaI ?? emp.fecha_i ?? '')
    };
    this.modalAbierto = true;
  }

  cerrarModal(): void {
    this.modalAbierto = false;
    this.empleadoSeleccionado = null;
  }

  guardarCambios(): void {
    if (!this.empleadoSeleccionado) return;
    //console.log('Llamando a editar con id:', this.empleadoSeleccionado.id);
    const payload = {
      ...this.empleadoSeleccionado,
      idRol: this.empleadoSeleccionado.idRol,
      fechaI: this.empleadoSeleccionado.fechaI
    };
    //console.log('payload completo:', JSON.stringify(payload));
    this.empleadoService.editar(this.empleadoSeleccionado.id, payload).subscribe({
      next: (data) => {
        console.log('Respuesta editar:', data);
        this.cargarEmpleados();
        this.cerrarModal();
      },

      error: (err) => {
        console.error('Error editando empleado', err);
      }
    });
  }
  // ELIMINAR PERSONAL
  modalEliminarAbierto: boolean = false;
  empleadoAEliminar: Empleado | null = null;
  claveEliminacion: string = '';
  errorClave: boolean = false;
  CLAVE_ADMIN = 'confirmar';; // Al integrar backend: validar en la API

  abrirModalEliminar(emp: Empleado): void {
    this.empleadoAEliminar = emp;
    this.claveEliminacion = '';
    this.errorClave = false;
    this.modalEliminarAbierto = true;
  }

  cerrarModalEliminar(): void {
    this.modalEliminarAbierto = false;
    this.empleadoAEliminar = null;
    this.claveEliminacion = '';
    this.errorClave = false;
  }

  confirmarEliminacion(): void {
    if (this.claveEliminacion.toLowerCase() !== 'confirmar') {
      this.errorClave = true;
      return;
    }
    this.empleadoService.eliminar(this.empleadoAEliminar!.id).subscribe({
      next: () => {
        this.cargarEmpleados();
        this.cerrarModalEliminar();
        this.mostrarNotificacion('Empleado eliminado correctamente.');
      },
      error: (err) => console.error('Error eliminando empleado', err)
    });
  }

  can(roles: string[]): boolean {
    return roles.includes(this.currentRole);
  }

  setRole(role: string): void {
    this.currentRole = role;
  }

  logout(): void {
    this.authService.cerrarSesion();
    this.router.navigate(['/']);
  }
}


