using SaraTouch.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.DTOs.TimeSlots
{
    public class UpdateTimeSlotRequest
    {
        [Required, EnumDataType(typeof(SlotStatus))]
        public SlotStatus? Status { get; set; }
    }
}
