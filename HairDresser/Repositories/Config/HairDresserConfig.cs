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
    public class HairDresserConfig : IEntityTypeConfiguration<HairDresser>
    {
        public void Configure(EntityTypeBuilder<HairDresser> builder)
        {
            builder.HasKey(h => h.HairDresserId);

            builder.Property(h => h.HairDresserName).IsRequired();
            builder.HasData(
                new HairDresser() { HairDresserId = 1, HairDresserName = "arif"},
                new HairDresser() { HairDresserId = 2, HairDresserName = "deneme"}

                );

        }
    }
}
