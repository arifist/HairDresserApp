using AutoMapper;
using Entities.Dtos;
using Entities.Models;
using Repositories.Contracts;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class WorkTimeManager: IWorkTimeService
    {
        private readonly IRepositoryManager _manager;
        private readonly IMapper _mapper;


        public WorkTimeManager(IRepositoryManager manager, IMapper mapper)
        {
            _manager = manager;
            _mapper = mapper;
        }

        //public WorkTime? GetOneWorkTime(int id, bool trackChanges)
        //{
        //    var workTime = _manager.WorkTime.GetOneWorkTime(id, trackChanges);
        //    if (workTime is null)
        //        throw new Exception("workTime not found!");
        //    return workTime;
        //}

        public void UpdateOneWorkTime(WorkTimeDtoForUpdate workTimeDto)
        {
            var entity = _mapper.Map<WorkTime>(workTimeDto);
            _manager.WorkTime.UpdateOneWorkTime(entity);
            _manager.Save();
        }

        //public WorkTimeDtoForUpdate GetOneWorkTimeForUpdate(int id, bool trakcChanges)
        //{
        //    var workTime = GetOneWorkTime(id, trakcChanges);
        //    var workTimeDto = _mapper.Map<WorkTimeDtoForUpdate>(workTime);
        //    return workTimeDto;
        //}


    }
}
