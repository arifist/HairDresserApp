using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T>
            where T : class, new()
    {
        protected readonly RepositoryContext _context;

        protected RepositoryBase(RepositoryContext context)
        {
            _context = context;

        }


        public void Create(T entity)
        {
            _context.Set<T>().Add(entity);
        }

    }
}
