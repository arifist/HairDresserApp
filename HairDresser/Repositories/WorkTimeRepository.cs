using Entities.Models;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class WorkTimeRepository: RepositoryBase<WorkTime>, IWorkTimeRepository
    {
        private readonly List<WorkTime> _workTime = new List<WorkTime>();

        public WorkTimeRepository(RepositoryContext context) : base(context)
        {
        }
        public void UpdateOneWorkTime(WorkTime entity) => Update(entity);

        //public WorkTime? GetOneWorkTime(int id, bool trackChanges)
        //{
        //    return FindByCondition(p => p.WorkTimeId.Equals(id), trackChanges);
        //}
        public async Task<WorkTime> GetWorkTimeAsync(int id)
        {
            return await _context.WorkTimes.FindAsync(id);
        }
    }
}
