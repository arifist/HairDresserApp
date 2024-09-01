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

        private readonly IAuthService _authService;
        private readonly IWorkTimeService _workTimeService;

        public ServiceManager(IReservationService reservationService ,IAuthService authService,IWorkTimeService workTimeService)
        {
            _reservationService = reservationService;
            _authService = authService;
            _workTimeService = workTimeService;

        }

        public IReservationService ReservationService => _reservationService;


        public IAuthService AuthService => _authService;

        public IWorkTimeService WorkTimeService => _workTimeService;




    }
}
