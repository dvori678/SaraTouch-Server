using SaraTouch.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Entities
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public SlotStatus Status { get; set; } = SlotStatus.Available;
        // Concurrency token. מוגדר כ-token ב-Fluent API בפרויקט Data,
        // ומתחלף אוטומטית בכל שמירה. כך Core לא תלוי ב-EF.
        public Guid Version { get; set; } = Guid.NewGuid();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }

}
