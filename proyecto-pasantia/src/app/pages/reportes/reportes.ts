import { Component, OnInit, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ReporteService } from '../../services/reporte.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-reportes',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './reportes.html',
  styleUrl: './reportes.css',
})
export class Reportes implements OnInit, AfterViewInit {

  constructor(
    private authService: AuthService,
    private reporteService: ReporteService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (!this.authService.estaAutenticado()) {
      this.router.navigate(['/']);
      return;
    }
    if (!this.authService.tienePermiso('Ver datos sensibles')) {
      this.router.navigate(['/dashboard']);
      return;
    }
    //this.aplicarTema();

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

  ngAfterViewInit(): void {
    this.cargarGraficos();
  }

  
  tienePermiso(permiso: string): boolean {
    return this.authService.tienePermiso(permiso);
  }

  get currentRole(): string { return this.authService.getRol() ?? ''; }

  get user() {
    return {
      name: this.authService.getNombre() ?? '',
      initials: (this.authService.getNombre() ?? 'U').substring(0, 2).toUpperCase(),
      badge: this.currentRole,
    };
  }

  logout(): void {
    this.authService.cerrarSesion();
    this.router.navigate(['/']);
  }

  cargarGraficos(): void {
    this.reporteService.getPorDepartamento().subscribe({
      next: (data) => this.crearBarras('chartDepto', data.map(d => d.departamento), data.map(d => d.total), 'Empleados por Departamento'),
      error: (err) => console.error(err)
    });

    this.reporteService.getPorRol().subscribe({
      next: (data) => this.crearPie('chartRol', data.map(d => d.rol), data.map(d => d.total)),
      error: (err) => console.error(err)
    });

    this.reporteService.getPorGenero().subscribe({
      next: (data) => this.crearDonut('chartGenero', data.map(d => d.genero), data.map(d => d.total)),
      error: (err) => console.error(err)
    });

    this.reporteService.getPorEstado().subscribe({
      next: (data) => this.crearBarras('chartEstado', data.map(d => d.estado), data.map(d => d.total), 'Empleados por Estado'),
      error: (err) => console.error(err)
    });

    this.reporteService.getSueldoPorDepartamento().subscribe({
      next: (data) => this.crearBarrasHorizontales('chartSueldo', data.map(d => d.departamento), data.map(d => Math.round(d.promedio))),
      error: (err) => console.error(err)
    });

    this.reporteService.getIngresosPorAnio().subscribe({
      next: (data) => this.crearLinea('chartIngresos', data.map(d => d.anio.toString()), data.map(d => d.total)),
      error: (err) => console.error(err)
    });
  }

  colores = ['#1d4ed8','#16a34a','#dc2626','#d97706','#9333ea','#0891b2','#be185d','#065f46'];

  crearBarras(id: string, labels: string[], data: number[], label: string): void {
    new Chart(id, {
      type: 'bar',
      data: {
        labels,
        datasets: [{ label, data, backgroundColor: this.colores, borderRadius: 4 }]
      },
      options: { responsive: true, plugins: { legend: { display: false } } }
    });
  }

  crearBarrasHorizontales(id: string, labels: string[], data: number[]): void {
    new Chart(id, {
      type: 'bar',
      data: {
        labels,
        datasets: [{ label: 'Sueldo Promedio', data, backgroundColor: this.colores, borderRadius: 4 }]
      },
      options: { indexAxis: 'y', responsive: true, plugins: { legend: { display: false } } }
    });
  }

  crearPie(id: string, labels: string[], data: number[]): void {
    new Chart(id, {
      type: 'pie',
      data: { labels, datasets: [{ data, backgroundColor: this.colores }] },
      options: { responsive: true }
    });
  }

  crearDonut(id: string, labels: string[], data: number[]): void {
    new Chart(id, {
      type: 'doughnut',
      data: { labels, datasets: [{ data, backgroundColor: this.colores }] },
      options: { responsive: true }
    });
  }

  crearLinea(id: string, labels: string[], data: number[]): void {
    new Chart(id, {
      type: 'line',
      data: {
        labels,
        datasets: [{
          label: 'Contrataciones',
          data,
          borderColor: '#1d4ed8',
          backgroundColor: 'rgba(29,78,216,0.1)',
          fill: true,
          tension: 0.4
        }]
      },
      options: { responsive: true }
    });
  }
}
