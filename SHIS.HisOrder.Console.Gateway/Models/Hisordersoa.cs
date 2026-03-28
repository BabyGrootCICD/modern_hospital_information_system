using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.Console.Gateway.Models;

public partial class Hisordersoa
{
    public long Soaid { get; set; }

    public string Inhospid { get; set; } = null!;

    public string HealthId { get; set; } = null!;

    public string Kind { get; set; } = null!;

    public string? Context { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string SourceType { get; set; } = null!;

    public int? VersionCode { get; set; }

    public char? Status { get; set; }

    public string? DcUser { get; set; }

    public DateTime? DcDate { get; set; }

    public string? ModifyUser { get; set; }

    public DateTime? ModifyDate { get; set; }
}
