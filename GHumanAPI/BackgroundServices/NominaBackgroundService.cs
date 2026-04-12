using GHumanAPI.Data;
using GHumanAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace GHumanAPI.BackgroundServices
{
    public class NominaBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NominaBackgroundService> _logger;

        public NominaBackgroundService(IServiceScopeFactory scopeFactory, ILogger<NominaBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await VerificarNomina();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task VerificarNomina()
        {
            try
            {
                var ahora = DateTime.Now;
                var diaActual = ahora.Day;
                var horaActual = ahora.Hour;
                var mesActual = ahora.Month;

                // Verificar si es fin de febrero
                var ultimoDiaFebrero = DateTime.DaysInMonth(ahora.Year, 2);
                var esDiaDePago = diaActual == 30 || (ahora.Month == 2 && diaActual == ultimoDiaFebrero);

                if (!esDiaDePago || horaActual < 8)
                {
                    _logger.LogInformation($"Nómina no procesada. Día: {diaActual}, Hora: {horaActual}");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var nominaService = scope.ServiceProvider.GetRequiredService<INominaService>();

                var config = await context.EmpresaConfig.FirstOrDefaultAsync();
                if (config == null) return;

                if (config.UltimaNominaMes == mesActual)
                {
                    _logger.LogInformation($"Nómina del mes {mesActual} ya fue procesada.");
                    return;
                }

                _logger.LogInformation("Procesando nómina...");
                await nominaService.ProcesarNomina();
                _logger.LogInformation("✅ Nómina procesada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en NominaBackgroundService: {ex.Message}");
            }
        }
    }
}