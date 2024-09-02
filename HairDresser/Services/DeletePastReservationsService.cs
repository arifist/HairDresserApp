using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class DeletePastReservationsService : BackgroundService, IDeletePastReservationsService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // IServiceScopeFactory kullanarak scoped servisleri çağırmak için scope yaratıyoruz.
        public DeletePastReservationsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // BackgroundService sınıfının zorunlu olan ExecuteAsync metodunu override ediyoruz.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Servis çalışırken belirli aralıklarla görev yürütüyoruz.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DeletePastReservationsAsync();
                    // Burada bekleme süresini belirliyoruz, örneğin her 24 saatte bir çalışması için:
                    //await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

                }
                catch (Exception ex)
                {
                    // Hata yönetimi, loglama işlemleri burada yapılabilir.
                    // Örneğin: Log hata mesajını kaydet.
                }
            }
        }


        private async Task DeletePastReservationsAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();
                await reservationService.DeletePastReservationsAsync(); // Asenkron silme işlemi
            }
        }

    }
}

