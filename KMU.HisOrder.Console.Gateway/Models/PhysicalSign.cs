using System;
using System.Collections.Generic;

namespace KMU.HisOrder.Console.Gateway.Models;

public partial class PhysicalSign
{
    public string PhyId { get; set; } = null!;

    public string Inhospid { get; set; } = null!;

    public string PhyType { get; set; } = null!;

    public string? PhyValue { get; set; }

    public string? ModifyUser { get; set; }

    public DateTime? ModifyTime { get; set; }
}
