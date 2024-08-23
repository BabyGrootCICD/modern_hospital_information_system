using System;
using System.Collections.Generic;

namespace KMU.MOHD.WebAPI.Models;

public partial class MohdChart
{
    public string FromHosp { get; set; }

    public string HealthId { get; set; }

    public string NationalId { get; set; }

    public string FName { get; set; }

    public string MName { get; set; }

    public string LName { get; set; }

    public string Sex { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string Mobile { get; set; }

    public string Address { get; set; }

    public string EmgCont { get; set; }

    public string ContRel { get; set; }

    public string ContPhone { get; set; }

    public char? CombineFlag { get; set; }

    public string Remark { get; set; }

    public DateTime? ModifyTime { get; set; }

    public string AreaCode { get; set; }

    public char? RefugeeFlag { get; set; }

    public DateTime? UploadTime { get; set; }
}
