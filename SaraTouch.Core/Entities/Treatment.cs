using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Entities
{
    public class Treatment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }

}
