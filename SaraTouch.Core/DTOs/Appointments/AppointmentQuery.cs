using SaraTouch.Core.Common;
using SaraTouch.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.DTOs.Appointments
{
    public class AppointmentQuery : PagedQuery
    {
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public AppointmentStatus? Status { get; set; }
    }
}
