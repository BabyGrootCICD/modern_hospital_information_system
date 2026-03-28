using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace SHIS.Lab.WebAPI.Models;

public partial class ShisChart
{
    public string HealthId { get; set; } = null!;


    public string PatientFirstname { get; set; } = null!;

    public string? PatientMidname { get; set; }

    public string? PatientLastname { get; set; }

    public string Sex { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    public string MobilePhone { get; set; } = null!;

    public string? Address { get; set; }
    public DateTime ModifyTime { get; set; }

    
}
