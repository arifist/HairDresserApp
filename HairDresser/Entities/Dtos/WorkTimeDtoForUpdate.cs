using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public record WorkTimeDtoForUpdate
    {
        [Required(ErrorMessage = "ID zorunludur")]
        public int WorkTimeId { get; init; }

        [Required(ErrorMessage = "Başlangıç zamanı zorunludur")]
        public DateTime WorkStartTime { get; set; }

        [Required(ErrorMessage = "Bitiş zamanı zorunludur")]
        public DateTime WorkEndTime { get; set; }
    }
}
