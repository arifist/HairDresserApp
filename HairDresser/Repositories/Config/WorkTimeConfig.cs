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
    public class WorkTimeConfig : IEntityTypeConfiguration<WorkTime>
    {
        public void Configure(EntityTypeBuilder<WorkTime> builder)
        {
            builder.HasKey(r => r.WorkTimeId);

            builder.HasData(
                new WorkTime() { WorkTimeId = 1, WorkStartTime = new DateTime(2024, 08, 06, 09, 00, 00), WorkEndTime = new DateTime(2024, 08, 06, 22, 00, 00) }

                );

        }
    }
}
