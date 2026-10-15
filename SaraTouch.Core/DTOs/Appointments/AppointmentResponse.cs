using SaraTouch.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.DTOs.Appointments
{
    public class AppointmentResponse
    {
        public int Id { get; set; }
        public AppointmentStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public int TimeSlotId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        public int TreatmentId { get; set; }
        public string TreatmentName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int UserId { get; set; }
        public string ClientName { get; set; } = string.Empty;
    }
}
