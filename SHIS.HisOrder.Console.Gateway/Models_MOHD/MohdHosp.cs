using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.Console.Gateway.Models_MOHD;

public partial class MohdHosp
{
    public string HospCode { get; set; } = null!;

    public string? HospName { get; set; }

    public string? HospAddress { get; set; }

    public string? HospTel { get; set; }

    public string? HospUploadKey { get; set; }
}
