using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Config
{
    public class CustomerConfig : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.CustomerId);

            builder.Property(c => c.CustomerName).IsRequired();
            builder.HasData(
                new Customer() { CustomerId = 1, CustomerName = "arif", PhoneNumber = "0545545454" },
                new Customer() { CustomerId = 2, CustomerName = "deneme", PhoneNumber = "6465421" }

                );

        }
    }
}
