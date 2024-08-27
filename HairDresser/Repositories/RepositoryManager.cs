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
        private readonly ICustomerRepository _customerRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IHairDresserRepository _hairDresserRepository;
        private readonly IWorkTimeRepository _workTimeRepository;


        public RepositoryManager(RepositoryContext context, ICustomerRepository customerRepository, IReservationRepository reservationRepository, IHairDresserRepository hairDresserRepository, IWorkTimeRepository workTimeRepository)
        {
            _context = context;
            _customerRepository = customerRepository;
            _reservationRepository = reservationRepository;
            _hairDresserRepository = hairDresserRepository;
            _workTimeRepository = workTimeRepository;

        }


        public ICustomerRepository Customer => _customerRepository;

        public IHairDresserRepository HairDresser => _hairDresserRepository;

        public IReservationRepository Reservation => _reservationRepository;

        public IWorkTimeRepository WorkTime => _workTimeRepository;

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
