using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.Console.Gateway.Models;

public partial class ShisMedpathway
{
    public char MedType { get; set; }

    public string PathCode { get; set; } = null!;

    public string PathDesc { get; set; } = null!;

    public decimal? Showseq { get; set; }

    public char EnableStatus { get; set; }

    public string? CreateUser { get; set; }

    public string? ModifyUser { get; set; }
}
