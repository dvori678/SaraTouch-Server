using SaraTouch.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int TreatmentId { get; set; }
        public Treatment Treatment { get; set; } = null!;
        public int TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; } = null!;
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Confirmed;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }
}
