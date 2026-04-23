using Microsoft.EntityFrameworkCore;
using GHumanAPI.Models;

namespace GHumanAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RolPermiso> RolesPermisos { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<DatosSensible> Datos_sensibles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<EmpresaConfig> EmpresaConfig { get; set; }
        public DbSet<NominaPago> NominaPagos { get; set; }
        public DbSet<Postulante> Postulantes { get; set; }
        public DbSet<FiltroAts> FiltrosAts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. MAPEAMOS LAS TABLAS A MINÚSCULAS (Para evitar el error 42P01 en Postgres)
            modelBuilder.Entity<Empleado>().ToTable("empleados");
            modelBuilder.Entity<Rol>().ToTable("roles");
            modelBuilder.Entity<Permiso>().ToTable("permisos");
            modelBuilder.Entity<RolPermiso>().ToTable("rolespermisos"); // Recomendado usar snake_case
            modelBuilder.Entity<DatosSensible>().ToTable("datos_sensibles");
            modelBuilder.Entity<Usuario>().ToTable("usuarios");

            // Mapeo de columnas Empleados
            modelBuilder.Entity<Empleado>(e =>
            {
                e.Property(x => x.FechaI).HasColumnName("fecha_i");
                e.Property(x => x.IdJefe).HasColumnName("id_jefe");
                e.Property(x => x.IdRol).HasColumnName("id_rol");
                e.Property(x => x.Departamento).HasColumnName("departamento");
                e.Property(x => x.Genero).HasColumnName("genero");
                e.Property(x => x.Nombre).HasColumnName("nombre");
                e.Property(x => x.Email).HasColumnName("email");
                e.Property(x => x.Estado).HasColumnName("estado");
                e.Property(x => x.Sueldo).HasColumnName("sueldo");
                e.Property(x => x.Ausencias).HasColumnName("ausencias");
            });

            // Mapeo de columnas Usuarios
            modelBuilder.Entity<Usuario>(u =>
            {
                u.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
                u.Property(x => x.PasswordHash).HasColumnName("password_hash");
                u.Property(x => x.PinHash).HasColumnName("pin_hash");
                u.Property(x => x.Username).HasColumnName("username"); // No olvides esta si existe en tu modelo
            });

            // Mapeo de columnas DatosSensibles
            modelBuilder.Entity<DatosSensible>(ds =>
            {
                ds.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
                ds.Property(x => x.TipoDocumento).HasColumnName("tipo_documento");
                ds.Property(x => x.NumDocumento).HasColumnName("num_documento");
                ds.Property(x => x.TipoSangre).HasColumnName("tipo_sangre");
                ds.Property(x => x.FechaNacimiento).HasColumnName("fecha_nacimiento");
                ds.Property(x => x.ContactoEmergencia).HasColumnName("contacto_emergencia");
                ds.Property(x => x.TelEmergencia).HasColumnName("tel_emergencia");
            });

            // Mapeo de columnas RolPermiso
            modelBuilder.Entity<RolPermiso>(rp =>
            {
                rp.Property(x => x.IdRol).HasColumnName("id_rol");
                rp.Property(x => x.IdPermiso).HasColumnName("id_permiso");
            });

            // --- CONFIGURACIÓN DE RELACIONES ---

            // Clave compuesta RolPermiso
            modelBuilder.Entity<RolPermiso>()
                .HasKey(rp => new { rp.IdRol, rp.IdPermiso });

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Rol)
                .WithMany(r => r.RolesPermisos)
                .HasForeignKey(rp => rp.IdRol);

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolesPermisos)
                .HasForeignKey(rp => rp.IdPermiso);

            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Jefe)
                .WithMany(e => e.Subordinados)
                .HasForeignKey(e => e.IdJefe)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Rol)
                .WithMany(r => r.Empleados)
                .HasForeignKey(e => e.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DatosSensible>()
                .HasOne(ds => ds.Empleado)
                .WithOne(e => e.DatosSensibles)
                .HasForeignKey<DatosSensible>(ds => ds.IdEmpleado)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Empleado)
                .WithOne(e => e.Usuario)
                .HasForeignKey<Usuario>(u => u.IdEmpleado)
                .OnDelete(DeleteBehavior.NoAction);

            // 2. REFUERZO AUTOMÁTICO PARA MINÚSCULAS (Cualquier propiedad/columna no mapeada arriba)
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    // Si no le hemos puesto un nombre manual, lo pasamos a minúsculas
                    property.SetColumnName(property.GetColumnName().ToLower());
                }
            }
            modelBuilder.Entity<EmpresaConfig>().ToTable("empresa_config");
            modelBuilder.Entity<NominaPago>().ToTable("nomina_pagos");

            modelBuilder.Entity<EmpresaConfig>(ec =>
           {
               ec.Property(x => x.BalanceActual).HasColumnName("balance_actual").HasColumnType("decimal(18,2)");
               ec.Property(x => x.UltimaNominaMes).HasColumnName("ultima_nomina_mes");
               ec.Property(x => x.DiaPago).HasColumnName("dia_pago");
               ec.Property(x => x.EmailAdmin).HasColumnName("email_admin");
               ec.Property(x => x.SmtpPasswordHash).HasColumnName("smtp_password_hash");
               ec.Property(x => x.TestRunKeyHash).HasColumnName("test_run_key_hash");
               ec.Property(x => x.LimiteAusencias).HasColumnName("limiteausencias");
           });

            modelBuilder.Entity<NominaPago>(np =>
            {
                np.Property(x => x.EmpleadoId).HasColumnName("empleado_id");
                np.Property(x => x.FechaPago).HasColumnName("fecha_pago");
                np.Property(x => x.MontoBase).HasColumnName("monto_base").HasColumnType("decimal(18,2)");
                np.Property(x => x.MontoBono).HasColumnName("monto_bono").HasColumnType("decimal(18,2)");
                np.Property(x => x.MontoTotal).HasColumnName("monto_total").HasColumnType("decimal(18,2)");
                np.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.EmpleadoId);
            });

            modelBuilder.Entity<Empleado>(e =>
            {
                e.Property(x => x.BonoProximoPago).HasColumnName("bono_proximo_pago");
                e.Property(x => x.Banco).HasColumnName("banco");
                e.Property(x => x.NumeroCuenta).HasColumnName("numero_cuenta");
                e.Property(x => x.TipoCuenta).HasColumnName("tipo_cuenta");
                e.Property(x => x.Ausencias).HasColumnName("ausencias");
            });

            //filtros ats
            modelBuilder.Entity<Postulante>().ToTable("postulantes");
            modelBuilder.Entity<FiltroAts>().ToTable("filtros_ats");

            modelBuilder.Entity<Postulante>(p =>
            {
                p.Property(x => x.PuestoAplicado).HasColumnName("puesto_aplicado");
                p.Property(x => x.ExperienciaAnios).HasColumnName("experiencia_anios");
                p.Property(x => x.NivelEducacion).HasColumnName("nivel_educacion");
                p.Property(x => x.CvUrl).HasColumnName("cv_url");
                p.Property(x => x.CvTexto).HasColumnName("cv_texto");
                p.Property(x => x.FechaPostulacion).HasColumnName("fecha_postulacion");
                p.Property(x => x.Notas).HasColumnName("notas");
            });

            modelBuilder.Entity<FiltroAts>(f =>
            {
                f.Property(x => x.PalabrasClave).HasColumnName("palabras_clave");
                f.Property(x => x.Activo).HasColumnName("activo");
                f.Property(x => x.CreadoEn).HasColumnName("creado_en");
            });
        }
    }
}