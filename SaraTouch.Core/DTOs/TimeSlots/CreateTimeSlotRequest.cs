using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.DTOs.TimeSlots
{
    public class CreateTimeSlotRequest
    {
        [Required]
        public DateTime? StartUtc { get; set; }

        [Range(15, 480)]
        public int DurationMinutes { get; set; } = 60;
    }
}
