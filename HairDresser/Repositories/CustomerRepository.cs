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
    public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
    {

        public CustomerRepository(RepositoryContext context) : base(context)
        {
        }
    }
}
