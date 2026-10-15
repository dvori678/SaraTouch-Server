using SaraTouch.Core.Common;
using SaraTouch.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.DTOs.TimeSlots
{
    public class TimeSlotQuery: PagedQuery
    {
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public SlotStatus? Status { get; set; }
    }
}
