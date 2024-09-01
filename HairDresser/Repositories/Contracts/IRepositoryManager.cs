using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryManager
    {

        IReservationRepository Reservation { get; }
        IWorkTimeRepository WorkTime { get; }
        void Save();

    }
}
