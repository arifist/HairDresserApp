using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public record ReservationDtoForUpdate: ReservationDto
    {
        public bool Showcase { get; set; }

    }
}
