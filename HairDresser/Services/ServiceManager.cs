using Repositories.Contracts;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly IReservationService _reservationService;
        private readonly ICustomerService _customerService;
        private readonly IHairDresserService _hairDresserService;

        public ServiceManager(IReservationService reservationService ,ICustomerService customerService, IHairDresserService hairDresserService)
        {
            _reservationService = reservationService;
            _customerService = customerService;
            _hairDresserService = hairDresserService;
        }

        public IReservationService ReservationService => _reservationService;

        public IHairDresserService HairDresserService => _hairDresserService;

        public ICustomerService CustomerService => _customerService;





    }
}
