using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.Console.Gateway.Models;

/// <summary>
/// Diagnosis data.
/// </summary>
public partial class ShisIcd
{
    public string IcdCode { get; set; } = null!;

    public string IcdEnglishName { get; set; } = null!;

    /// <summary>
    /// ICD Code without decimal point.
    /// </summary>
    public string IcdCodeUndot { get; set; } = null!;

    /// <summary>
    /// Parent ICD Code for HisOrder UI Design.
    /// </summary>
    public string? ParentCode { get; set; }

    /// <summary>
    /// ICD Code.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// CM/PCS
    /// </summary>
    public string? IcdType { get; set; }

    /// <summary>
    /// Show position for HisOrder UI Design.
    /// </summary>
    public string? ShowMode { get; set; }

    /// <summary>
    /// ICD 9 / ICD 10 ...
    /// </summary>
    public string? Versioncode { get; set; }

    public string ModifyUser { get; set; } = null!;

    public DateTime ModifyDate { get; set; }

    public int? Dhis2Code { get; set; }
}
