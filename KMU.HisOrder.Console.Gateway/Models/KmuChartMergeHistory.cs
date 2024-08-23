using System;
using System.Collections.Generic;

namespace KMU.HisOrder.Console.Gateway.Models;

public partial class KmuChartMergeHistory
{
    public int Id { get; set; }

    public string? ChrHalthId { get; set; }

    public string? MhHealthId { get; set; }

    //public DateTime CreateTime { get; set; }

    public DateTime MergedTime { get; set; }

    public string? MergerUser { get; set; }

    public string? UploadStatus { get; set; }

    public DateTime? UploadTime { get; set; }
}
