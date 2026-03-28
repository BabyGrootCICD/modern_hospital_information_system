using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.Console.Gateway.Models;

public partial class ShisMedfrequency
{
    public string FrqCode { get; set; } = null!;

    public string FreqDesc { get; set; } = null!;

    public int? FrqForDays { get; set; }

    public int? FrqForTimes { get; set; }

    public int? FrqOneDayTimes { get; set; }

    public char EnableStatus { get; set; }

    public string? CreateUser { get; set; }

    public string? ModifyUser { get; set; }

    public int? FrqSeqNo { get; set; }
}
