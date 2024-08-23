using System;
using System.Collections.Generic;

namespace KMU.HisOrder.MVC.Models
{
    public partial class Hisordersoa
    {
        public long Soaid { get; set; }
        public string Inhospid { get; set; }
        public string HealthId { get; set; }
        public string Kind { get; set; }
        public string Context { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string SourceType { get; set; }
        public int? VersionCode { get; set; }
        public char? Status { get; set; }
        public string DcUser { get; set; }
        public DateTime? DcDate { get; set; }
        public string ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
