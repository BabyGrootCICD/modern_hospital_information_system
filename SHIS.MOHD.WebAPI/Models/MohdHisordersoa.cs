using System;
using System.Collections.Generic;

namespace SHIS.MOHD.WebAPI.Models;

public partial class MohdHisordersoa
{
    public string HospCode { get; set; }

    public long Soaid { get; set; }

    public string Inhospid { get; set; }

    public string HealthId { get; set; }

    public string Kind { get; set; }

    public string Context { get; set; }

    public DateTime? CreateTime { get; set; }

    public string SourceType { get; set; }

    public int? VersionCode { get; set; }

    public char? Status { get; set; }

    public DateTime? ModifyTime { get; set; }
}
