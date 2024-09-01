using Entities.Dtos;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _context;
        private readonly IReservationRepository _reservationRepository;
        private readonly IWorkTimeRepository _workTimeRepository;

        public RepositoryManager(RepositoryContext context, IReservationRepository reservationRepository, IWorkTimeRepository workTimeRepository)
        {
            _context = context;
            _reservationRepository = reservationRepository;
            _workTimeRepository = workTimeRepository;

        }



        public IReservationRepository Reservation => _reservationRepository;

        public IWorkTimeRepository WorkTime => _workTimeRepository;

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
