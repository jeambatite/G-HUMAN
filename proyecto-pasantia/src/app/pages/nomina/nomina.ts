import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NominaService } from '../../services/nomina.service';

@Component({
  selector: 'app-nomina',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './nomina.html',
  styleUrl: './nomina.css',
})
export class Nomina implements OnInit {

  empleados: any[] = [];
  config: any = null;
  historial: any[] = [];
  notificacion: { mensaje: string; tipo: 'exito' | 'error' } | null = null;
  bonoGlobal: number = 0;
  filtros = { nombre: '', departamento: '' };
  private filtroTimeout: any = null;

  // Modal test-run
  modalTestRunAbierto: boolean = false;
  secretKey: string = '';
  errorTestRun: string = '';
  procesando: boolean = false;

  //variables paginado
  paginaActual: number = 1;
  tamanoPagina: number = 10;
  totalEmpleados: number = 0;
  totalPaginas: number = 0;
  opcionesTamano: number[] = [10, 25, 50, 100];

  //metodos paginado
  get empleadosPaginados(): any[] {
    const filtrados = this.empleadosFiltrados;
    this.totalEmpleados = filtrados.length;
    this.totalPaginas = Math.ceil(filtrados.length / this.tamanoPagina);
    const inicio = (this.paginaActual - 1) * this.tamanoPagina;
    return filtrados.slice(inicio, inicio + this.tamanoPagina);
  }

  cambiarPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas) return;
    this.paginaActual = pagina;
    this.cdr.detectChanges();
  }

  cambiarTamano(tamano: number): void {
    this.tamanoPagina = tamano;
    this.paginaActual = 1;
    this.cdr.detectChanges();
  }

  get paginas(): (number | string)[] {
    const total = this.totalPaginas;
    const actual = this.paginaActual;

    if (total <= 15) return Array.from({ length: total }, (_, i) => i + 1);

    const result: (number | string)[] = [];
    for (let i = 1; i <= 10; i++) result.push(i);

    if (actual > 10 && actual <= total - 5) {
      result.push('...');
      for (let i = actual - 1; i <= actual + 1; i++) result.push(i);
    }

    result.push('...');
    for (let i = total - 4; i <= total; i++) result.push(i);

    return [...new Set(result)];
  }
  //
  constructor(
    private authService: AuthService,
    private nominaService: NominaService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

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
    this.cargarDatos();

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

  cargarDatos(): void {
    this.nominaService.getConfig().subscribe({
      next: (data) => { this.config = data; this.cdr.detectChanges(); },
      error: (err) => console.error('Error cargando config', err)
    });

    this.nominaService.getEmpleados().subscribe({
      next: (data) => { this.empleados = data; this.cdr.detectChanges(); },
      error: (err) => console.error('Error cargando empleados', err)
    });

    this.nominaService.getHistorial().subscribe({
      next: (data) => { this.historial = data; this.cdr.detectChanges(); },
      error: (err) => console.error('Error cargando historial', err)
    });
  }

  get empleadosFiltrados(): any[] {
    return this.empleados.filter(e => {
      const matchNombre = !this.filtros.nombre || e.nombre.toLowerCase().includes(this.filtros.nombre.toLowerCase());
      const matchDepto = !this.filtros.departamento || e.departamento === this.filtros.departamento;
      return matchNombre && matchDepto;
    });
  }

  get deptosUnicos(): string[] {
    return [...new Set(this.empleados.map(e => e.departamento).filter(Boolean))] as string[];
  }

  get totalNomina(): number {
    return this.empleados.reduce((acc, emp) => acc + emp.sueldo, 0);
  }

  onFiltroChange(): void {
    if (this.filtroTimeout) clearTimeout(this.filtroTimeout);
    this.filtroTimeout = setTimeout(() => { this.cdr.detectChanges(); }, 500);
  }

  limpiarFiltros(): void {
    this.filtros = { nombre: '', departamento: '' };
    this.cdr.detectChanges();
  }

  hayFiltrosActivos(): boolean {
    return Object.values(this.filtros).some(v => v !== '');
  }

  soloEnteros(event: KeyboardEvent): boolean {
    const charCode = event.which ?? event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
      event.preventDefault();
      return false;
    }
    return true;
  }

  aplicarBonoGlobal(): void {
    if (this.bonoGlobal < 0 || this.bonoGlobal > 100) {
      this.mostrarNotificacion('El bono debe ser un número entero entre 0 y 100.', 'error');
      return;
    }
    this.nominaService.bonoGlobal(this.bonoGlobal).subscribe({
      next: () => {
        this.empleados = this.empleados.map(e => ({ ...e, bonoProximoPago: this.bonoGlobal }));
        this.mostrarNotificacion(`Bono de ${this.bonoGlobal}% aplicado a todos los empleados.`);
        this.bonoGlobal = 0;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error aplicando bono global', err)
    });
  }

  guardarBono(emp: any): void {
    if (emp.bonoProximoPago < 0 || emp.bonoProximoPago > 100) {
      this.mostrarNotificacion('El bono debe ser entre 0 y 100.', 'error');
      return;
    }
    this.nominaService.actualizarBono(emp.id, emp.bonoProximoPago).subscribe({
      next: () => this.mostrarNotificacion(`Bono de ${emp.bonoProximoPago}% guardado para ${emp.nombre}.`),
      error: (err) => console.error('Error guardando bono', err)
    });
  }

  calcularTotal(emp: any): number {
    const bono = emp.sueldo * (emp.bonoProximoPago / 100);
    return emp.sueldo + bono;
  }
  guardarTodosLosBonos(): void {
    const promesas = this.empleados
      .filter(e => e.bonoProximoPago > 0)
      .map(e => this.nominaService.actualizarBono(e.id, e.bonoProximoPago).toPromise());

    Promise.all(promesas).then(() => {
      this.mostrarNotificacion('Bonos guardados correctamente.');
    }).catch(() => {
      this.mostrarNotificacion('Error guardando algunos bonos.', 'error');
    });
  }


  // Modal test-run
  abrirModalTestRun(): void {
    this.secretKey = '';
    this.errorTestRun = '';
    this.modalTestRunAbierto = true;
  }

  cerrarModalTestRun(): void {
    this.modalTestRunAbierto = false;
    this.secretKey = '';
  }

  ejecutarTestRun(): void {
    if (!this.secretKey) {
      this.errorTestRun = 'La clave es obligatoria.';
      return;
    }
    this.errorTestRun = '';
    this.procesando = true;
    this.cdr.detectChanges();
    this.nominaService.testRun(this.secretKey).subscribe({
      next: () => {
        this.procesando = false;
        this.cerrarModalTestRun();
        this.mostrarNotificacion('Nómina procesada correctamente.');
        this.cargarDatos();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.procesando = false;
        if (err.status === 401) {
          this.errorTestRun = 'Clave incorrecta.';
        } else {
          this.errorTestRun = 'Error al procesar la nómina.';
        }
        this.cdr.detectChanges();
      }
    });
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