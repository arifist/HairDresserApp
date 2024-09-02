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
        private readonly IDeletePastReservationsService _deletePastReservationsService;

        public ServiceManager(IReservationService reservationService ,IAuthService authService,IWorkTimeService workTimeService, IDeletePastReservationsService deletePastReservationsService)
        {
            _reservationService = reservationService;
            _authService = authService;
            _workTimeService = workTimeService;
            _deletePastReservationsService= deletePastReservationsService;
        }

        public IReservationService ReservationService => _reservationService;

        public IAuthService AuthService => _authService;

        public IWorkTimeService WorkTimeService => _workTimeService;

        public IDeletePastReservationsService DeletePastReservationsService => _deletePastReservationsService;
    }
}
