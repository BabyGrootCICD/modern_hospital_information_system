using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.Console.Gateway.Models_MOHD;

public partial class MohdHisordersoa
{
    public string HospCode { get; set; } = null!;

    public long Soaid { get; set; }

    public string? Inhospid { get; set; }

    public string? HealthId { get; set; }

    public string? Kind { get; set; }

    public string? Context { get; set; }

    public DateTime? CreateTime { get; set; }

    public string? SourceType { get; set; }

    public int? VersionCode { get; set; }

    public char? Status { get; set; }

    public DateTime? ModifyTime { get; set; }
}
