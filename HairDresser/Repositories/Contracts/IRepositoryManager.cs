using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryManager
    {
        ICustomerRepository Customer { get; }
        IHairDresserRepository HairDresser { get; }
        IReservationRepository Reservation { get; }

        void Save();

    }
}
