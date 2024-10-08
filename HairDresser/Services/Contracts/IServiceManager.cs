﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IServiceManager
    {
        IReservationService ReservationService { get; }
        IAuthService AuthService { get; }
        IWorkTimeService WorkTimeService { get; }
        IDeletePastReservationsService DeletePastReservationsService { get; }
    }
}
