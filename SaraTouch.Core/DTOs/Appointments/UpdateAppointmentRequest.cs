using SaraTouch.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.DTOs.Appointments
{
    public class UpdateAppointmentRequest
    {
        [Required, EnumDataType(typeof(AppointmentStatus))]
        public AppointmentStatus? Status { get; set; }
    }
}
