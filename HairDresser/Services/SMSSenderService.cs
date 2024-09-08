using HairDresserApp.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Services
{
    public class SMSSenderService: ISMSSenderService
    {
        private readonly TwilioSettings _twilioSettings;

        public SMSSenderService(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
        }
        public async Task SendSMSAsync(string toPhone, string message)
        {

            try
            {
                // SMS gönderme kodu
                TwilioClient.Init(_twilioSettings.AccountSID, _twilioSettings.AuthToken);

                await MessageResource.CreateAsync(
                    to: toPhone,
                    from: _twilioSettings.FromPhone,
                    body: message
                );
            }
            catch (Exception ex)
            {
                // Hata günlüğü
                // Logger ile hata mesajını günlüğe kaydedin
                Console.WriteLine($"SMS gönderimi sırasında hata oluştu: {ex.Message}");
            }

        }
    }
}
