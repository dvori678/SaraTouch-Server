using SaraTouch.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.DTOs.Treatments
{
    public class TreatmentQuery : PagedQuery
    {
        public string? Search { get; set; }
        public int? TagId { get; set; }
        public string SortBy { get; set; } = "name";   // name | price
        public bool Descending { get; set; }
    }
}
