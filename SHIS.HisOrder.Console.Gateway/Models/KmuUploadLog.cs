using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.Console.Gateway.Models;

public partial class ShisUploadLog
{
    public long Logid { get; set; }

    public DateOnly? RegDate { get; set; }

    public string? Inhospid { get; set; }

    public DateTime? ExecDatetime { get; set; }

    public char? Option { get; set; }

    public bool? ResultSuccess { get; set; }

    public string? ResultMessage { get; set; }

    public string? ResultStatusCode { get; set; }

    public string? ResultStatusDesc { get; set; }

    public string? TargetUrl { get; set; }

    public string? TargetAgency { get; set; }

    public string? LocalIp { get; set; }

    public string? LocalLoginUser { get; set; }

    public short? ExecBatchSeqNo { get; set; }
}
