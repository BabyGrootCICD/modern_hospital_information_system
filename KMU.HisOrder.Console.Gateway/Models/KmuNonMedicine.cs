using System;
using System.Collections.Generic;

namespace KMU.HisOrder.Console.Gateway.Models;

public partial class KmuNonMedicine
{
    public string ItemId { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    /// <summary>
    /// 5.Laboratory 6.Radiology 7.Pathology 8.Material
    /// </summary>
    public string ItemType { get; set; } = null!;

    public string? ItemSpec { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public char Status { get; set; }

    public string? CreateUser { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? ModifyUser { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? Remark { get; set; }

    public decimal? ShowSeq { get; set; }

    public string? GroupCode { get; set; }
}
