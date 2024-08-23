using System;
using System.Collections.Generic;

namespace KMU.HisOrder.Console.Gateway.Models;

public partial class KmuMedfrequencyInd
{
    public string FrqCode { get; set; } = null!;

    public string IndCode { get; set; } = null!;

    public string? IndDesc { get; set; }

    public decimal? Showseq { get; set; }

    public char EnableStatus { get; set; }

    public string? CreateUser { get; set; }

    public string? ModifyUser { get; set; }
}
