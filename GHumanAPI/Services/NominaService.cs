using Microsoft.EntityFrameworkCore;
using GHumanAPI.Data;
using GHumanAPI.DTOs;
using GHumanAPI.Models;
using System.Net;
using System.Net.Mail;
using Resend;

namespace GHumanAPI.Services
{
    public class NominaService : INominaService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        private readonly IResend _resend;


        public NominaService(AppDbContext context, IConfiguration config, IResend resend)
        {
            _context = context;
            _config = config;
            _resend = resend;
        }

        public async Task<List<NominaEmpleadoDTO>> GetEmpleadosNomina()
        {
            return await _context.Empleados
                .Include(e => e.Rol)
                .Where(e => e.Estado == "activo" || e.Estado == "vacaciones")
                .Select(e => new NominaEmpleadoDTO
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Departamento = e.Departamento,
                    NombreRol = e.Rol.Nombre,
                    Sueldo = e.Sueldo,
                    BonoProximoPago = e.BonoProximoPago,
                    Banco = e.Banco,
                    NumeroCuenta = e.NumeroCuenta,
                    TipoCuenta = e.TipoCuenta,
                    Estado = e.Estado
                })
                .ToListAsync();
        }

        public async Task<EmpresaConfigDTO?> GetConfig()
        {
            var config = await _context.EmpresaConfig.FirstOrDefaultAsync();
            if (config == null) return null;
            return new EmpresaConfigDTO
            {
                BalanceActual = config.BalanceActual,
                UltimaNominaMes = config.UltimaNominaMes,
                DiaPago = config.DiaPago,
                EmailAdmin = config.EmailAdmin,
                LimiteAusencias = config.LimiteAusencias
            };
        }

        public async Task<bool> ActualizarBono(ActualizarBonoDTO dto)
        {
            if (dto.Bono < 0 || dto.Bono > 100) return false;
            var empleado = await _context.Empleados.FindAsync(dto.EmpleadoId);
            if (empleado == null) return false;
            empleado.BonoProximoPago = dto.Bono;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BonoGlobal(BonoGlobalDTO dto)
        {
            if (dto.Bono < 0 || dto.Bono > 100) return false;
            var empleados = await _context.Empleados
                .Where(e => e.Estado == "activo")
                .ToListAsync();
            foreach (var emp in empleados)
                emp.BonoProximoPago = dto.Bono;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<NominaPagoDTO>> GetHistorial()
        {
            return await _context.NominaPagos
                .Include(n => n.Empleado)
                .OrderByDescending(n => n.FechaPago)
                .Select(n => new NominaPagoDTO
                {
                    Id = n.Id,
                    EmpleadoId = n.EmpleadoId,
                    NombreEmpleado = n.Empleado.Nombre,
                    FechaPago = n.FechaPago,
                    MontoBase = n.MontoBase,
                    MontoBono = n.MontoBono,
                    MontoTotal = n.MontoTotal
                })
                .ToListAsync();
        }

        public async Task ProcesarNomina()
        {
            var config = await _context.EmpresaConfig.FirstOrDefaultAsync();
            if (config == null) return;

            var empleados = await _context.Empleados
                .Include(e => e.Rol)
                .Where(e => e.Estado == "activo" || e.Estado == "vacaciones")
                .ToListAsync();

            var fallos = new List<int>();
            decimal totalDebitado = 0;

            foreach (var emp in empleados)
            {
                try
                {
                    decimal factorBono = emp.BonoProximoPago / 100.0m;
                    decimal montoBono = emp.Sueldo * factorBono;
                    decimal montoTotal = emp.Sueldo + montoBono;

                    // Registrar pago
                    _context.NominaPagos.Add(new NominaPago
                    {
                        EmpleadoId = emp.Id,
                        FechaPago = DateTime.UtcNow,
                        MontoBase = emp.Sueldo,
                        MontoBono = montoBono,
                        MontoTotal = montoTotal
                    });

                    // Descontar del balance
                    config.BalanceActual -= montoTotal;
                    totalDebitado += montoTotal;

                    // Resetear bono
                    emp.BonoProximoPago = 0;

                    await _context.SaveChangesAsync();

                    // Enviar email al empleado
                    await EnviarEmailEmpleado(emp, montoTotal, montoBono, config.EmailAdmin);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando empleado {emp.Id}: {ex.Message}");
                    fallos.Add(emp.Id);
                }
            }

            // Actualizar mes procesado
            config.UltimaNominaMes = DateTime.UtcNow.Month;
            await _context.SaveChangesAsync();

            // Alerta si balance negativo
            if (config.BalanceActual < 0)
                await EnviarEmailAdmin(config.EmailAdmin, $"⚠️ ALERTA: El balance de la empresa es negativo: ${config.BalanceActual:N2}");

            // Reporte final al admin
            await EnviarReporteAdmin(config.EmailAdmin, empleados.Count, totalDebitado, fallos);
        }

        private async Task EnviarEmailEmpleado(Empleado emp, decimal montoTotal, decimal montoBono, string emailAdmin)
        {
            var cuerpo = $@"
                <h2>Notificación de Pago - G-HUMAN</h2>
                <p>Estimado/a <strong>{emp.Nombre}</strong>,</p>
                <p>Se ha procesado su pago correspondiente al mes de {DateTime.UtcNow:MMMM yyyy}.</p>
                <table>
                    <tr><td>Sueldo Base:</td><td>${emp.Sueldo:N2}</td></tr>
                    <tr><td>Bono ({emp.BonoProximoPago}%):</td><td>${montoBono:N2}</td></tr>
                    <tr><td><strong>Total:</strong></td><td><strong>${montoTotal:N2}</strong></td></tr>
                </table>
                <p>Cuenta: {emp.Banco} - {emp.NumeroCuenta} ({emp.TipoCuenta})</p>
                <br><p>G-HUMAN Sistema de RRHH</p>
            ";
            await EnviarEmail(emp.Email, "Pago de Nómina Procesado - G-HUMAN", cuerpo, emailAdmin);
        }

        private async Task EnviarReporteAdmin(string emailAdmin, int total, decimal totalDebitado, List<int> fallos)
        {
            var cuerpo = $@"
                <h2>Reporte de Nómina - G-HUMAN</h2>
                <p>Se ha procesado la nómina del mes de {DateTime.UtcNow:MMMM yyyy}.</p>
                <ul>
                    <li>Total empleados procesados: {total - fallos.Count}</li>
                    <li>Monto total debitado: ${totalDebitado:N2}</li>
                    <li>Fallos: {(fallos.Count == 0 ? "Ninguno" : string.Join(", ", fallos))}</li>
                </ul>
            ";
            await EnviarEmail(emailAdmin, "Reporte de Nómina - G-HUMAN", cuerpo, emailAdmin);
        }

        private async Task EnviarEmailAdmin(string emailAdmin, string mensaje)
        {
            await EnviarEmail(emailAdmin, "Alerta G-HUMAN", mensaje, emailAdmin);
        }

        private async Task EnviarEmail(string destino, string asunto, string cuerpo, string emailAdmin)
        {
            try
            {
                var message = new EmailMessage
                {
                    From = $"G-HUMAN <onboarding@resend.dev>",
                    To = { destino },
                    Subject = asunto,
                    HtmlBody = cuerpo
                };

                await _resend.EmailSendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando email a {destino}: {ex.Message}");
            }
        }
        public async Task<bool> ActualizarLimiteAusencias(int limite)
        {
            if (limite < 1 || limite > 365) return false;

            var config = await _context.EmpresaConfig.FirstOrDefaultAsync();
            if (config == null) return false;

            config.LimiteAusencias = limite;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
