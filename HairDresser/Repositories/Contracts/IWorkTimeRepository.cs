using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IWorkTimeRepository :IRepositoryBase<WorkTime>
    {
        void UpdateOneWorkTime(WorkTime entity);
        //WorkTime? GetOneWorkTime(int id, bool trackChanges);

    }
}
