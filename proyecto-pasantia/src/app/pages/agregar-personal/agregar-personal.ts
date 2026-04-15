import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { EmpleadoService } from '../../services/empleado.service';
import { RolService } from '../../services/rol.service';

@Component({
  selector: 'app-agregar-personal',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './agregar-personal.html',
  styleUrl: './agregar-personal.css',
})
export class AgregarPersonal implements OnInit {

  roles: { id: number; nombre: string; permisos: string[]; idPermisos: number[] }[] = [];
  notificacion: { mensaje: string; tipo: 'exito' | 'error' } | null = null;

  departamentos: string[] = [
    'Ventas', 'Administración', 'Gerencia', 'Tecnología',
    'Marketing', 'Finanzas', 'Recursos Humanos'
  ];

  nuevoEmpleado: any = this.empleadoVacio();

  constructor(
    private authService: AuthService,
    private empleadoService: EmpleadoService,
    private rolService: RolService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    if (!this.authService.estaAutenticado()) {
      this.router.navigate(['/']);
      return;
    }
    if (!this.authService.tienePermiso('Agregar personal')) {
      this.router.navigate(['/dashboard']);
      return;
    }
    //this.aplicarTema();
    this.cargarRoles();

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

  

  tienePermiso(permiso: string): boolean {
    return this.authService.tienePermiso(permiso);
  }

  cargarRoles(): void {
    this.rolService.getAll().subscribe({
      next: (data) => { this.roles = data; this.cdr.detectChanges(); },
      error: (err) => console.error('Error cargando roles', err)
    });
  }

  empleadoVacio(): any {
    return {
      nombre: '', email: '', genero: '', idRol: 0, estado: 'activo',
      departamento: '', jefe: '', sueldo: 0, fechaI: '',
      tipoDocumento: '', numDocumento: '', tipoSangre: '',
      fechaNacimiento: '', telefono: '', contactoEmergencia: '', telEmergencia: '',
      banco: '', numeroCuenta: '', tipoCuenta: ''
    };
  }

  mostrarNotificacion(mensaje: string, tipo: 'exito' | 'error' = 'exito'): void {
    this.notificacion = { mensaje, tipo };
    this.cdr.detectChanges();
    setTimeout(() => { this.notificacion = null; this.cdr.detectChanges(); }, 3000);
  }

  agregarEmpleado(): void {
    const e = this.nuevoEmpleado;
    if (
      !e.nombre || !e.email || !e.idRol || !e.genero ||
      !e.estado || !e.departamento || !e.fechaI ||
      !e.tipoDocumento || !e.numDocumento || !e.tipoSangre ||
      !e.fechaNacimiento || !e.telefono || !e.contactoEmergencia || !e.telEmergencia
    ) {
      this.mostrarNotificacion('Todos los campos son obligatorios.', 'error');
      return;
    }
    this.empleadoService.crear(this.nuevoEmpleado).subscribe({
      next: () => {
        this.nuevoEmpleado = this.empleadoVacio();
        this.mostrarNotificacion('Empleado agregado correctamente.');
      },
      error: (err) => {
        if (err.status === 409) {
          this.mostrarNotificacion(err.error.message, 'error');
        } else {
          console.error('Error agregando empleado', err);
        }
      }
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