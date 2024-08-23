using System;
using System.Collections.Generic;

namespace KMU.HisOrder.Console.Gateway.Models;

/// <summary>
/// Auth Setting reference Project File(main function node)
/// </summary>
public partial class KmuProject
{
    public string ProjectId { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public string Url { get; set; } = null!;

    public string Creator { get; set; } = null!;

    public DateTime CreateTime { get; set; }
}
