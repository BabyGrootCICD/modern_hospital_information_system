using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.MVC.Models
{
    public partial class ShisSerialpool
    {
        public string SerialOwner { get; set; }
        public long? SerialNo { get; set; }
        public string SerialPrefix { get; set; }
        public string SerialMaxno { get; set; }
    }
}
