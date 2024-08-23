using System;
using System.Collections.Generic;

namespace KMU.HisOrder.MVC.Models
{
    public partial class PhysicalSign
    {
        public string PhyId { get; set; }
        public string Inhospid { get; set; }
        public string PhyType { get; set; }
        public string PhyValue { get; set; }
        public string ModifyUser { get; set; }
        public DateTime? ModifyTime { get; set; }
    }
}
