using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Config
{
    public class ReservationConfig: IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.HasKey(r => r.ReservationId);

            builder.HasData(
                new Reservation() { ReservationId = 1, CustomerId=1, ReservationName="arif", ReservationDay=DateTime.Now, HairCutTypes="saç ve sakal"},
                new Reservation() { ReservationId = 2, CustomerId=2, ReservationName = "mehmet" ,ReservationDay = DateTime.Now, HairCutTypes = "saç ve sakal" }

                );

        }
    }
}
