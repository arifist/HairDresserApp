using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IReservationService
    {

        void CreateReservation(ReservationDtoForInsertion reservationDto);

    }
}
