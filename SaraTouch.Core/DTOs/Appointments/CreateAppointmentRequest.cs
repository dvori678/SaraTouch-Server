using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.DTOs.Appointments
{
    public class CreateAppointmentRequest
    {
        [Range(1, int.MaxValue)]
        public int TimeSlotId { get; set; }

        [Range(1, int.MaxValue)]
        public int TreatmentId { get; set; }
        [StringLength(300)]
        public string? Notes { get; set; }
    }
}
