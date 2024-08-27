using Entities.Dtos;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IWorkTimeService
    {
        void UpdateOneWorkTime(WorkTimeDtoForUpdate workTimeDto);
        //WorkTimeDtoForUpdate GetOneWorkTimeForUpdate(int id, bool trackChanges);
        //WorkTime? GetOneWorkTime(int id, bool trackChanges);

    }
}
