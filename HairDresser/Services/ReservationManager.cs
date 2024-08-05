using AutoMapper;
using Entities.Dtos;
using Entities.Models;
using Repositories;
using Repositories.Contracts;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ReservationManager :IReservationService
    {

        private readonly IRepositoryManager _manager;
        private readonly IMapper _mapper;


        public ReservationManager(IRepositoryManager manager, IMapper mapper)
        {
            _manager = manager;
            _mapper = mapper;
        }



        public void CreateReservation(ReservationDtoForInsertion reservationDto)
        {
            Reservation reservation = _mapper.Map<Reservation>(reservationDto);
            _manager.Reservation.Create(reservation);
            _manager.Save();
        }
    }
}
